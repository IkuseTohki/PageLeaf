using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using PageLeaf.Models.Settings;
using System.Linq;
using System.Text;
using System.IO;
using PageLeaf.Utilities;
using System.Windows.Media; // ColorConverter
using AngleSharp.Css.Values; // AngleSharp.Css.Values.Color
using System.Collections.Generic; // List<string>
using System;
using AngleSharp.Css;
using System.Text.RegularExpressions;

namespace PageLeaf.Services
{
    public class CssEditorService : ICssEditorService
    {
        public CssStyleProfile ParseToProfile(string cssContent)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(cssContent);

            var profile = new CssStyleProfile();
            profile.UpdateFrom(stylesheet);

            return profile;
        }

        public CssStyleInfo ParseCss(string cssContent)
        {
            var profile = ParseToProfile(cssContent);
            var styleInfo = new CssStyleInfo();

            // プロファイルから StyleInfo (旧) へデータを同期
            SyncProfileToStyleInfo(profile, styleInfo);

            return styleInfo;
        }

        public string UpdateCssFromProfile(string existingCss, CssStyleProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(existingCss);

            // プロファイルを適用
            profile.ApplyTo(stylesheet);

            return FinalizeCssUpdate(stylesheet, profile);
        }

        public string UpdateCssContent(string existingCss, CssStyleInfo styleInfo)
        {
            if (styleInfo == null) throw new ArgumentNullException(nameof(styleInfo));

            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(existingCss);

            var profile = new CssStyleProfile();
            // まず既存のCSSをプロファイルに読み込む
            profile.UpdateFrom(stylesheet);

            // 次に StyleInfo (旧) からプロファイルへ同期（変更を上書き）
            SyncStyleInfoToProfile(styleInfo, profile);

            // 更新されたプロファイルを使用してCSSを生成
            // UpdateCssFromProfile 内でもパースが行われるため、重複を避けるためにロジックを共通化するか、
            // 内部メソッドを呼び出す。ここではシンプルに UpdateCssFromProfile のロジックを流用する。
            profile.ApplyTo(stylesheet);

            // 見出し採番、リスト、後処理などを実行
            // (UpdateCssFromProfile の残りの処理を実行)
            return FinalizeCssUpdate(stylesheet, profile);
        }

        private string FinalizeCssUpdate(ICssStyleSheet stylesheet, CssStyleProfile profile)
        {
            // 見出し採番の追加処理
            UpdateHeadingNumbering(stylesheet, profile);

            // リスト採番ルールの徹底クリーンアップ
            CleanupListBeforeRules(stylesheet);

            // 1. ol (list-style-type) の制御
            UpdateOrCreateRule(stylesheet, "ol", (rule, p) =>
            {
                if (p.List.OrderedListMarkerType == "decimal-nested" || p.List.HasOrderedListPeriod.HasValue)
                {
                    // 階層採番またはピリオド設定がある場合は、確実に none にする
                    rule.Style.SetProperty("list-style-type", "none");
                }
                else if (!string.IsNullOrEmpty(p.List.OrderedListMarkerType))
                {
                    // それ以外（decimal 等）
                    rule.Style.SetProperty("list-style-type", p.List.OrderedListMarkerType);
                }
            }, profile);

            // 2. li (display) の制御
            UpdateOrCreateRule(stylesheet, "li", (rule, p) =>
            {
                // 全体的な li に対する display 操作は行わず、各リストタイプごとの詳細ルールに任せる
                rule.Style.RemoveProperty("display");
            }, profile);

            // 3. li::before (汎用) のクリーンアップ
            UpdateOrCreateRule(stylesheet, "li::before", (rule, p) =>
            {
                rule.Style.RemoveProperty("content");
                rule.Style.RemoveProperty("counter-increment");
            }, profile);

            // 4. ol > li (詳細) の制御
            UpdateOrCreateRule(stylesheet, "ol > li", (rule, p) =>
            {
                if (p.List.OrderedListMarkerType == "decimal-nested")
                {
                    // 階層採番の場合のみ block にする (::before を正しく表示するため)
                    rule.Style.SetProperty("display", "block");
                }
                else
                {
                    rule.Style.RemoveProperty("display");
                }
            }, profile);

            // 5. ol > li::before (詳細) の制御
            // 階層採番またはピリオド設定がある場合のみ、この詳細ルールを制御（作成・更新）する
            if (profile.List.OrderedListMarkerType == "decimal-nested" || profile.List.HasOrderedListPeriod.HasValue)
            {
                UpdateOrCreateRule(stylesheet, "ol > li::before", (rule, p) =>
                {
                    var suffix = (p.List.HasOrderedListPeriod ?? false) ? "\". \"" : "\" \"";

                    // インデント調整用スタイル
                    rule.Style.SetProperty("display", "inline-block");
                    rule.Style.SetProperty("text-align", "right");
                    rule.Style.SetProperty("padding-right", "0.5em"); // マーカーとテキストの間隔

                    // ListIndent に基づくネガティブマージンと幅の設定
                    // デフォルトは 2em と仮定（またはブラウザデフォルトに合わせる）
                    var indent = p.List.ListIndent?.ToString() ?? "2em";
                    rule.Style.SetProperty("width", indent);
                    rule.Style.SetProperty("margin-left", $"-{indent}");

                    if (p.List.OrderedListMarkerType == "decimal-nested")
                    {
                        rule.Style.SetProperty("content", $"counters(item, \".\") {suffix}");
                        rule.Style.SetProperty("counter-increment", "item");
                    }
                    else
                    {
                        // 方式D: OrderedListMarkerType が decimal の場合は省略形 counter(list-item) を出力する
                        // それ以外（lower-alpha等）の場合は明示的にタイプを指定する
                        var type = (string.IsNullOrEmpty(p.List.OrderedListMarkerType) || p.List.OrderedListMarkerType == "decimal")
                                   ? "" : $", {p.List.OrderedListMarkerType}";
                        rule.Style.SetProperty("content", $"counter(list-item{type}) {suffix}");
                        rule.Style.RemoveProperty("counter-increment");
                    }
                }, profile);
            }
            else
            {
                // 未設定時は、もしルールが存在していれば content を消す（Cleanupで消えているはずだが念のため）
                var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r =>
                    r.SelectorText != null && r.SelectorText.Replace(" ", "") == "ol>li::before");
                rule?.Style.RemoveProperty("content");
            }

            // 5b. 脚注内のリスト採番干渉対策
            // 階層採番（decimal-nested）やピリオド設定が有効な場合、または一般のリスト形式が変更されている場合、
            // 脚注内のリスト番号にも影響が出てしまうため、脚注エリア内ではこれらを明示的に無効化し、
            // 標準の list-style-type (decimal) に強制する。
            if (profile.List.OrderedListMarkerType == "decimal-nested" ||
                profile.List.HasOrderedListPeriod.HasValue ||
                (!string.IsNullOrEmpty(profile.List.OrderedListMarkerType) && profile.List.OrderedListMarkerType != "decimal"))
            {
                UpdateOrCreateRule(stylesheet, ".footnotes ol", (rule, p) =>
                {
                    rule.Style.SetProperty("list-style-type", "decimal");
                    rule.Style.SetProperty("counter-reset", "none");
                }, profile);

                UpdateOrCreateRule(stylesheet, ".footnotes ol > li", (rule, p) =>
                {
                    rule.Style.SetProperty("display", "list-item");
                }, profile);

                UpdateOrCreateRule(stylesheet, ".footnotes ol > li::before", (rule, p) =>
                {
                    rule.Style.SetProperty("content", "none");
                }, profile);
            }

            // 6. その他
            UpdateOrCreateRule(stylesheet, "li::marker", (rule, p) =>
            {
                if (p.List.MarkerSize != null) rule.Style.SetProperty("font-size", p.List.MarkerSize.ToString());
            }, profile);

            // チェックボックス（タスクリスト）の場合はマーカーを消す
            UpdateOrCreateRule(stylesheet, "li:has(input[type=\"checkbox\"])", (rule, p) =>
            {
                rule.Style.SetProperty("font-weight", "normal");
                rule.Style.SetProperty("list-style-type", "none");
            }, profile);

            // 更新されたスタイルシートを文字列として出力
            string generatedCss;
            using (var writer = new StringWriter())
            {
                stylesheet.ToCss(writer, new PageLeaf.Utilities.PrettyStyleFormatter());
                generatedCss = writer.ToString();
            }

            // 後処理（ショートハンド化、counters構文修正、空ルール削除）
            return PostProcessCss(generatedCss, profile);
        }

        private void SyncProfileToStyleInfo(CssStyleProfile profile, CssStyleInfo styleInfo)
        {
            // Body
            styleInfo.BodyTextColor = profile.Body.TextColor?.ToString();
            styleInfo.BodyBackgroundColor = profile.Body.BackgroundColor?.ToString();
            styleInfo.BodyFontSize = profile.Body.FontSize?.ToString();
            styleInfo.BodyFontFamily = profile.Body.FontFamily;

            // Paragraph
            styleInfo.ParagraphLineHeight = profile.Paragraph.LineHeight;
            styleInfo.ParagraphMarginBottom = profile.Paragraph.MarginBottom?.ToString();
            styleInfo.ParagraphTextIndent = profile.Paragraph.TextIndent?.ToString();

            // Title
            styleInfo.TitleTextColor = profile.Title.TextColor?.ToString();
            styleInfo.TitleFontSize = profile.Title.FontSize?.ToString();
            styleInfo.TitleFontFamily = profile.Title.FontFamily;
            styleInfo.TitleAlignment = profile.Title.TextAlignment.ToCssString();
            styleInfo.TitleMarginBottom = profile.Title.MarginBottom?.ToString();
            styleInfo.TitleStyleFlags.IsBold = profile.Title.IsBold;
            styleInfo.TitleStyleFlags.IsItalic = profile.Title.TextStyle.IsItalic;
            styleInfo.TitleStyleFlags.IsUnderline = profile.Title.TextStyle.IsUnderline;

            // Headings
            styleInfo.HeadingTextColors.Clear();
            styleInfo.HeadingFontSizes.Clear();
            styleInfo.HeadingFontFamilies.Clear();
            styleInfo.HeadingAlignments.Clear();
            styleInfo.HeadingMarginTops.Clear();
            styleInfo.HeadingMarginBottoms.Clear();

            foreach (var level in new[] { "h1", "h2", "h3", "h4", "h5", "h6" })
            {
                var h = profile.Headings[level];

                // オブジェクト同期
                var targetH = styleInfo.Headings[level];
                targetH.TextColor = h.TextColor;
                targetH.FontSize = h.FontSize;
                targetH.FontFamily = h.FontFamily;
                targetH.TextAlignment = h.TextAlignment;
                targetH.MarginTop = h.MarginTop;
                targetH.MarginBottom = h.MarginBottom;
                targetH.IsBold = h.IsBold;
                targetH.IsItalic = h.IsItalic;
                targetH.IsUnderline = h.IsUnderline;

                var textColor = h.TextColor?.ToString();
                if (textColor != null) styleInfo.HeadingTextColors[level] = textColor;

                var fontSize = h.FontSize?.ToString();
                if (fontSize != null) styleInfo.HeadingFontSizes[level] = fontSize;

                if (h.FontFamily != null) styleInfo.HeadingFontFamilies[level] = h.FontFamily;

                var alignment = h.TextAlignment.ToCssString() ?? "left";
                styleInfo.HeadingAlignments[level] = alignment;

                var marginTop = h.MarginTop?.ToString();
                if (marginTop != null) styleInfo.HeadingMarginTops[level] = marginTop;

                var marginBottom = h.MarginBottom?.ToString();
                if (marginBottom != null) styleInfo.HeadingMarginBottoms[level] = marginBottom;

                styleInfo.HeadingStyleFlags[level] = new HeadingStyleFlags
                {
                    IsBold = h.IsBold,
                    IsItalic = h.IsItalic,
                    IsUnderline = h.IsUnderline
                };

                if (profile.HeadingNumberingStates.TryGetValue(level, out var n))
                    styleInfo.HeadingNumberingStates[level] = n;
            }

            // Blockquote
            styleInfo.QuoteTextColor = profile.Blockquote.TextColor?.ToString();
            styleInfo.QuoteBackgroundColor = profile.Blockquote.BackgroundColor?.ToString();
            styleInfo.QuoteBorderWidth = profile.Blockquote.BorderWidth;
            styleInfo.QuoteBorderStyle = profile.Blockquote.BorderStyle;
            styleInfo.QuotePadding = profile.Blockquote.Padding;
            styleInfo.QuoteBorderRadius = profile.Blockquote.BorderRadius;
            styleInfo.Blockquote.IsItalic = profile.Blockquote.IsItalic;
            styleInfo.Blockquote.ShowIcon = profile.Blockquote.ShowIcon;

            var quoteBorderColor = profile.Blockquote.BorderColor?.ToString();
            if (quoteBorderColor != null) styleInfo.QuoteBorderColor = quoteBorderColor;

            // List
            styleInfo.ListMarkerType = profile.List.UnorderedListMarkerType;
            styleInfo.NumberedListMarkerType = profile.List.OrderedListMarkerType;
            styleInfo.HasNumberedListPeriod = profile.List.HasOrderedListPeriod;
            styleInfo.ListIndent = profile.List.ListIndent?.ToString();
            styleInfo.ListMarkerSize = profile.List.MarkerSize?.ToString();
            styleInfo.ListLineHeight = profile.List.LineHeight;

            // Table
            styleInfo.TableBorderWidth = profile.Table.BorderWidth;

            var tableBorderColor = profile.Table.BorderColor?.ToString();
            if (tableBorderColor != null) styleInfo.TableBorderColor = tableBorderColor;

            styleInfo.TableBorderStyle = profile.Table.BorderStyle;
            styleInfo.TableCellPadding = profile.Table.CellPadding;
            styleInfo.TableWidth = profile.Table.Width;
            styleInfo.TableHeaderBackgroundColor = profile.Table.HeaderBackgroundColor?.ToString();
            styleInfo.TableHeaderTextColor = profile.Table.HeaderTextColor?.ToString();
            styleInfo.TableHeaderFontSize = profile.Table.HeaderFontSize;
            styleInfo.TableHeaderAlignment = profile.Table.HeaderAlignment;

            // Code
            styleInfo.CodeTextColor = profile.Code.TextColor?.ToString();
            styleInfo.CodeBackgroundColor = profile.Code.BackgroundColor?.ToString();
            styleInfo.InlineCodeTextColor = profile.Code.TextColor?.ToString();
            styleInfo.InlineCodeBackgroundColor = profile.Code.BackgroundColor?.ToString();
            styleInfo.CodeFontFamily = profile.Code.FontFamily;
            styleInfo.BlockCodeTextColor = profile.Code.BlockTextColor?.ToString();
            styleInfo.BlockCodeBackgroundColor = profile.Code.BlockBackgroundColor?.ToString();
            styleInfo.IsCodeBlockOverrideEnabled = profile.IsCodeBlockOverrideEnabled;

            // Footnote
            styleInfo.FootnoteMarkerTextColor = profile.Footnote.MarkerTextColor?.ToString();
            styleInfo.Footnote.MarkerTextColor = profile.Footnote.MarkerTextColor;
            styleInfo.Footnote.IsMarkerBold = profile.Footnote.IsMarkerBold;
            styleInfo.Footnote.HasMarkerBrackets = profile.Footnote.HasMarkerBrackets;

            styleInfo.FootnoteAreaFontSize = profile.Footnote.AreaFontSize?.ToString();
            styleInfo.Footnote.AreaFontSize = profile.Footnote.AreaFontSize;

            styleInfo.FootnoteAreaTextColor = profile.Footnote.AreaTextColor?.ToString();
            styleInfo.Footnote.AreaTextColor = profile.Footnote.AreaTextColor;

            styleInfo.FootnoteAreaMarginTop = profile.Footnote.AreaMarginTop?.ToString();
            styleInfo.Footnote.AreaMarginTop = profile.Footnote.AreaMarginTop;

            var footnoteBorderColor = profile.Footnote.AreaBorderTopColor?.ToString();
            if (footnoteBorderColor != null)
            {
                styleInfo.FootnoteAreaBorderTopColor = footnoteBorderColor;
                styleInfo.Footnote.AreaBorderTopColor = profile.Footnote.AreaBorderTopColor;
            }

            styleInfo.FootnoteAreaBorderTopWidth = profile.Footnote.AreaBorderTopWidth;
            styleInfo.Footnote.AreaBorderTopWidth = profile.Footnote.AreaBorderTopWidth;

            styleInfo.FootnoteAreaBorderTopStyle = profile.Footnote.AreaBorderTopStyle ?? "solid";
            styleInfo.Footnote.AreaBorderTopStyle = profile.Footnote.AreaBorderTopStyle ?? "solid";

            styleInfo.FootnoteListItemLineHeight = profile.Footnote.ListItemLineHeight ?? "1";
            styleInfo.Footnote.ListItemLineHeight = profile.Footnote.ListItemLineHeight ?? "1";

            styleInfo.Footnote.IsBackLinkVisible = profile.Footnote.IsBackLinkVisible;
        }

        private void SyncStyleInfoToProfile(CssStyleInfo styleInfo, CssStyleProfile profile)
        {
            // Body
            if (!string.IsNullOrEmpty(styleInfo.BodyTextColor)) profile.Body.TextColor = CssColor.Parse(styleInfo.BodyTextColor!);
            if (!string.IsNullOrEmpty(styleInfo.BodyBackgroundColor)) profile.Body.BackgroundColor = CssColor.Parse(styleInfo.BodyBackgroundColor!);
            if (!string.IsNullOrEmpty(styleInfo.BodyFontSize)) profile.Body.FontSize = CssSize.Parse(styleInfo.BodyFontSize!);
            if (styleInfo.BodyFontFamily != null) profile.Body.FontFamily = styleInfo.BodyFontFamily;

            // Paragraph
            if (!string.IsNullOrEmpty(styleInfo.ParagraphLineHeight)) profile.Paragraph.LineHeight = styleInfo.ParagraphLineHeight;
            if (!string.IsNullOrEmpty(styleInfo.ParagraphMarginBottom)) profile.Paragraph.MarginBottom = CssSize.Parse(styleInfo.ParagraphMarginBottom!);
            if (!string.IsNullOrEmpty(styleInfo.ParagraphTextIndent)) profile.Paragraph.TextIndent = CssSize.Parse(styleInfo.ParagraphTextIndent!);

            // Title
            if (!string.IsNullOrEmpty(styleInfo.TitleTextColor)) profile.Title.TextColor = CssColor.Parse(styleInfo.TitleTextColor!);
            if (!string.IsNullOrEmpty(styleInfo.TitleFontSize)) profile.Title.FontSize = CssSize.Parse(styleInfo.TitleFontSize!);
            if (styleInfo.TitleFontFamily != null) profile.Title.FontFamily = styleInfo.TitleFontFamily;
            if (!string.IsNullOrEmpty(styleInfo.TitleAlignment)) profile.Title.TextAlignment = CssAlignmentExtensions.Parse(styleInfo.TitleAlignment);
            if (!string.IsNullOrEmpty(styleInfo.TitleMarginBottom)) profile.Title.MarginBottom = CssSize.Parse(styleInfo.TitleMarginBottom!);

            // フラグは構造上常に値を持つため、デフォルト(false)との区別が難しいが、
            // 旧モデルの仕様に合わせる。
            profile.Title.IsBold = styleInfo.TitleStyleFlags.IsBold;
            profile.Title.TextStyle.IsItalic = styleInfo.TitleStyleFlags.IsItalic;
            profile.Title.TextStyle.IsUnderline = styleInfo.TitleStyleFlags.IsUnderline;

            // Headings
            foreach (var level in new[] { "h1", "h2", "h3", "h4", "h5", "h6" })
            {
                var h = profile.Headings[level];

                // 大文字小文字を区別せずに辞書から取得を試みる
                string? color = styleInfo.HeadingTextColors.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(color)) h.TextColor = CssColor.Parse(color.Trim());

                string? size = styleInfo.HeadingFontSizes.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(size)) h.FontSize = CssSize.Parse(size.Trim());

                string? family = styleInfo.HeadingFontFamilies.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(family)) h.FontFamily = family.Trim();

                string? align = styleInfo.HeadingAlignments.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(align)) h.TextAlignment = CssAlignmentExtensions.Parse(align.Trim());

                string? mt = styleInfo.HeadingMarginTops.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(mt)) h.MarginTop = CssSize.Parse(mt.Trim());

                string? mb = styleInfo.HeadingMarginBottoms.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(mb)) h.MarginBottom = CssSize.Parse(mb.Trim());

                var flagsKvp = styleInfo.HeadingStyleFlags.FirstOrDefault(x => x.Key.Equals(level, StringComparison.OrdinalIgnoreCase));
                if (flagsKvp.Key != null && flagsKvp.Value != null)
                {
                    var flags = flagsKvp.Value;
                    h.IsBold = flags.IsBold;
                    h.IsItalic = flags.IsItalic;
                    h.IsUnderline = flags.IsUnderline;
                }

                if (styleInfo.HeadingNumberingStates.TryGetValue(level, out var n)) profile.HeadingNumberingStates[level] = n;
            }

            // Blockquote
            if (!string.IsNullOrEmpty(styleInfo.QuoteTextColor)) profile.Blockquote.TextColor = CssColor.Parse(styleInfo.QuoteTextColor!);
            if (!string.IsNullOrEmpty(styleInfo.QuoteBackgroundColor)) profile.Blockquote.BackgroundColor = CssColor.Parse(styleInfo.QuoteBackgroundColor!);
            if (!string.IsNullOrEmpty(styleInfo.QuoteBorderWidth)) profile.Blockquote.BorderWidth = styleInfo.QuoteBorderWidth;
            if (!string.IsNullOrEmpty(styleInfo.QuoteBorderStyle)) profile.Blockquote.BorderStyle = styleInfo.QuoteBorderStyle;
            if (!string.IsNullOrEmpty(styleInfo.QuoteBorderColor)) profile.Blockquote.BorderColor = CssColor.Parse(styleInfo.QuoteBorderColor!);
            if (!string.IsNullOrEmpty(styleInfo.QuotePadding)) profile.Blockquote.Padding = styleInfo.QuotePadding;
            if (!string.IsNullOrEmpty(styleInfo.QuoteBorderRadius)) profile.Blockquote.BorderRadius = styleInfo.QuoteBorderRadius;
            profile.Blockquote.IsItalic = styleInfo.Blockquote.IsItalic;
            profile.Blockquote.ShowIcon = styleInfo.Blockquote.ShowIcon;

            // List
            if (!string.IsNullOrEmpty(styleInfo.ListMarkerType)) profile.List.UnorderedListMarkerType = styleInfo.ListMarkerType;
            if (!string.IsNullOrEmpty(styleInfo.NumberedListMarkerType)) profile.List.OrderedListMarkerType = styleInfo.NumberedListMarkerType;
            profile.List.HasOrderedListPeriod = styleInfo.HasNumberedListPeriod;
            if (!string.IsNullOrEmpty(styleInfo.ListIndent)) profile.List.ListIndent = CssSize.Parse(styleInfo.ListIndent!);
            if (!string.IsNullOrEmpty(styleInfo.ListMarkerSize)) profile.List.MarkerSize = CssSize.Parse(styleInfo.ListMarkerSize!);
            if (!string.IsNullOrEmpty(styleInfo.ListLineHeight)) profile.List.LineHeight = styleInfo.ListLineHeight;

            // Table
            if (!string.IsNullOrEmpty(styleInfo.TableBorderWidth)) profile.Table.BorderWidth = styleInfo.TableBorderWidth;
            if (!string.IsNullOrEmpty(styleInfo.TableBorderColor)) profile.Table.BorderColor = CssColor.Parse(styleInfo.TableBorderColor!);
            if (!string.IsNullOrEmpty(styleInfo.TableBorderStyle)) profile.Table.BorderStyle = styleInfo.TableBorderStyle;
            if (!string.IsNullOrEmpty(styleInfo.TableCellPadding)) profile.Table.CellPadding = styleInfo.TableCellPadding;
            if (!string.IsNullOrEmpty(styleInfo.TableWidth)) profile.Table.Width = styleInfo.TableWidth;
            if (!string.IsNullOrEmpty(styleInfo.TableHeaderBackgroundColor)) profile.Table.HeaderBackgroundColor = CssColor.Parse(styleInfo.TableHeaderBackgroundColor!);
            if (!string.IsNullOrEmpty(styleInfo.TableHeaderTextColor)) profile.Table.HeaderTextColor = CssColor.Parse(styleInfo.TableHeaderTextColor!);
            if (!string.IsNullOrEmpty(styleInfo.TableHeaderFontSize)) profile.Table.HeaderFontSize = styleInfo.TableHeaderFontSize;
            if (!string.IsNullOrEmpty(styleInfo.TableHeaderAlignment)) profile.Table.HeaderAlignment = styleInfo.TableHeaderAlignment;

            // Code
            string? codeColor = !string.IsNullOrEmpty(styleInfo.InlineCodeTextColor) ? styleInfo.InlineCodeTextColor : styleInfo.CodeTextColor;
            if (!string.IsNullOrEmpty(codeColor)) profile.Code.TextColor = CssColor.Parse(codeColor);

            string? codeBg = !string.IsNullOrEmpty(styleInfo.InlineCodeBackgroundColor) ? styleInfo.InlineCodeBackgroundColor : styleInfo.CodeBackgroundColor;
            if (!string.IsNullOrEmpty(codeBg)) profile.Code.BackgroundColor = CssColor.Parse(codeBg);

            if (styleInfo.CodeFontFamily != null) profile.Code.FontFamily = styleInfo.CodeFontFamily;
            if (!string.IsNullOrEmpty(styleInfo.BlockCodeTextColor)) profile.Code.BlockTextColor = CssColor.Parse(styleInfo.BlockCodeTextColor!);
            if (!string.IsNullOrEmpty(styleInfo.BlockCodeBackgroundColor)) profile.Code.BlockBackgroundColor = CssColor.Parse(styleInfo.BlockCodeBackgroundColor!);
            profile.IsCodeBlockOverrideEnabled = styleInfo.IsCodeBlockOverrideEnabled;

            // Footnote
            if (!string.IsNullOrEmpty(styleInfo.FootnoteMarkerTextColor)) profile.Footnote.MarkerTextColor = CssColor.Parse(styleInfo.FootnoteMarkerTextColor!);

            // Flags
            profile.Footnote.IsMarkerBold = styleInfo.Footnote.IsMarkerBold;
            profile.Footnote.HasMarkerBrackets = styleInfo.Footnote.HasMarkerBrackets;
            profile.Footnote.IsBackLinkVisible = styleInfo.Footnote.IsBackLinkVisible;

            if (!string.IsNullOrEmpty(styleInfo.FootnoteAreaFontSize)) profile.Footnote.AreaFontSize = CssSize.Parse(styleInfo.FootnoteAreaFontSize!);
            if (!string.IsNullOrEmpty(styleInfo.FootnoteAreaTextColor)) profile.Footnote.AreaTextColor = CssColor.Parse(styleInfo.FootnoteAreaTextColor!);
            if (!string.IsNullOrEmpty(styleInfo.FootnoteAreaMarginTop)) profile.Footnote.AreaMarginTop = CssSize.Parse(styleInfo.FootnoteAreaMarginTop!);
            if (!string.IsNullOrEmpty(styleInfo.FootnoteAreaBorderTopColor)) profile.Footnote.AreaBorderTopColor = CssColor.Parse(styleInfo.FootnoteAreaBorderTopColor!);
            if (!string.IsNullOrEmpty(styleInfo.FootnoteAreaBorderTopWidth)) profile.Footnote.AreaBorderTopWidth = styleInfo.FootnoteAreaBorderTopWidth;
            if (!string.IsNullOrEmpty(styleInfo.FootnoteAreaBorderTopStyle)) profile.Footnote.AreaBorderTopStyle = styleInfo.FootnoteAreaBorderTopStyle;
            if (!string.IsNullOrEmpty(styleInfo.FootnoteListItemLineHeight)) profile.Footnote.ListItemLineHeight = styleInfo.FootnoteListItemLineHeight;
        }
        private string PostProcessCss(string generatedCss, CssStyleProfile profile)
        {
            // 後処理で th, td スタイルをショートハンドに置換
            var thTdBlockPattern = @"th,\s*td\s*\{[^\}]+\}";
            var match = Regex.Match(generatedCss, thTdBlockPattern, RegexOptions.Singleline);

            if (match.Success)
            {
                var newStyles = new List<string>();
                var effectiveBorder = profile.Table.GetEffectiveBorder();
                newStyles.Add($"  border: {effectiveBorder};");

                if (!string.IsNullOrEmpty(profile.Table.CellPadding))
                {
                    newStyles.Add($"  padding: {profile.Table.CellPadding};");
                }

                if (newStyles.Any())
                {
                    var newBlockContent = string.Join(Environment.NewLine, newStyles);
                    var newBlock = $"th, td {{{Environment.NewLine}{newBlockContent}{Environment.NewLine}}}";
                    generatedCss = Regex.Replace(generatedCss, thTdBlockPattern, newBlock);
                }
            }

            // Fix for AngleSharp malformed counters syntax
            generatedCss = generatedCss.Replace("counters(item .)", "counters(item, \".\")");

            // Final cleanup: remove empty rules (e.g. "h4 {}")
            // セレクター部分に改行が含まれるケースや、複雑なセレクターも考慮
            generatedCss = Regex.Replace(generatedCss, @"(?m)^[^{}\r\n]+\s*\{\s*\}[\r\n]*", "");
            // Remove excessive newlines caused by empty rule removal
            generatedCss = Regex.Replace(generatedCss, @"(\r?\n){3,}", Environment.NewLine + Environment.NewLine);

            return generatedCss.Trim();
        }

        private void UpdateHeadingNumbering(ICssStyleSheet stylesheet, CssStyleProfile profile)
        {
            if (stylesheet?.Rules == null || profile == null) return;

            ClearExistingNumberingRules(stylesheet);
            UpdateBodyCounterReset(stylesheet, profile);
            UpdateHeadingCounterRules(stylesheet, profile);
            UpdateHeadingBeforeRules(stylesheet, profile);
        }

        private void UpdateHeadingCounterRules(ICssStyleSheet stylesheet, CssStyleProfile profile)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                UpdateOrCreateRule<CssStyleProfile>(stylesheet, selector, (rule, p) =>
                {
                    bool isEnabled = p.HeadingNumberingStates != null &&
                                   p.HeadingNumberingStates.TryGetValue(selector, out bool val) && val;

                    if (isEnabled)
                    {
                        rule.Style.SetProperty("counter-increment", selector);
                        if (i < 6) rule.Style.SetProperty("counter-reset", $"h{i + 1} 0");
                    }
                    else
                    {
                        rule.Style.RemoveProperty("counter-increment");
                        rule.Style.RemoveProperty("counter-reset");
                    }
                }, profile);
            }
        }

        private void UpdateHeadingBeforeRules(ICssStyleSheet stylesheet, CssStyleProfile profile)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                bool isEnabled = profile.HeadingNumberingStates != null &&
                               profile.HeadingNumberingStates.TryGetValue(selector, out bool val) && val;

                if (isEnabled)
                {
                    UpdateOrCreateRule<CssStyleProfile>(stylesheet, $"{selector}::before", (rule, p) =>
                    {
                        rule.Style.SetProperty("content", BuildCounterContent(i, p));
                    }, profile);
                }
            }
        }

        private string BuildCounterContent(int level, CssStyleProfile profile)
        {
            var sb = new StringBuilder();
            bool isFirst = true;
            for (int j = 1; j <= level; j++)
            {
                var h = $"h{j}";
                if (profile.HeadingNumberingStates != null && profile.HeadingNumberingStates.TryGetValue(h, out bool val) && val)
                {
                    if (!isFirst) sb.Append("\".\"");
                    sb.Append($"counter({h})");
                    isFirst = false;
                }
            }
            sb.Append("\". \"");
            return sb.ToString();
        }

        private void ClearExistingNumberingRules(ICssStyleSheet stylesheet)
        {
            for (int i = stylesheet.Rules.Length - 1; i >= 0; i--)
            {
                var rule = stylesheet.Rules[i];
                if (rule is ICssStyleRule styleRule && styleRule.SelectorText != null &&
                    styleRule.SelectorText.StartsWith("h") && styleRule.SelectorText.Contains("::before"))
                {
                    stylesheet.RemoveAt(i);
                }
            }
        }

        private void UpdateBodyCounterReset(ICssStyleSheet stylesheet, CssStyleProfile profile)
        {
            bool anyEnabled = profile.HeadingNumberingStates != null &&
                             profile.HeadingNumberingStates.Any(kvp => kvp.Value);

            UpdateOrCreateRule<CssStyleProfile>(stylesheet, "body", (rule, p) =>
            {
                if (anyEnabled)
                {
                    rule.Style.SetProperty("counter-reset", "h1 0");
                }
                else
                {
                    rule.Style.RemoveProperty("counter-reset");
                }
            }, profile);
        }

        private void CleanupListBeforeRules(ICssStyleSheet stylesheet)
        {
            for (int i = stylesheet.Rules.Length - 1; i >= 0; i--)
            {
                var rule = stylesheet.Rules[i];
                if (rule is ICssStyleRule styleRule && styleRule.SelectorText != null)
                {
                    var normalized = styleRule.SelectorText.Replace(" ", "");
                    if (normalized == "li::before" || normalized == "ol>li::before")
                    {
                        stylesheet.RemoveAt(i);
                    }
                }
            }
        }

        public string CreateNewStyle(string styleName)
        {
            throw new NotImplementedException();
        }

        private string? GetColorHexFromRule(ICssStyleRule rule, string propertyName)
        {
            var property = rule.Style.GetProperty(propertyName);
            if (property != null && property.RawValue is AngleSharp.Css.Values.Color angleSharpColor)
            {
                return $"#{angleSharpColor.R:X2}{angleSharpColor.G:X2}{angleSharpColor.B:X2}";
            }
            // Fallback for non-raw color values
            var colorValue = rule.Style.GetPropertyValue(propertyName);
            if (string.IsNullOrEmpty(colorValue) || colorValue == "transparent")
            {
                return null;
            }
            if (colorValue.StartsWith("#"))
            {
                return colorValue.ToUpper();
            }
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorValue);
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch
            {
                return null;
            }
        }

        private HeadingStyleFlags GetStyleFlags(ICssStyleRule rule)
        {
            var flags = new HeadingStyleFlags();
            var fontWeight = rule.Style.GetPropertyValue("font-weight");
            if (fontWeight == "bold" || (int.TryParse(fontWeight, out var weight) && weight >= 700))
            {
                flags.IsBold = true;
            }

            var fontStyle = rule.Style.GetPropertyValue("font-style");
            if (fontStyle == "italic")
            {
                flags.IsItalic = true;
            }

            var textDecoration = rule.Style.GetPropertyValue("text-decoration");
            if (textDecoration != null && textDecoration.Contains("underline"))
            {
                flags.IsUnderline = true;
            }
            return flags;
        }

        private void UpdateOrCreateRule<T>(ICssStyleSheet stylesheet, string selector, Action<ICssStyleRule, T> setProperties, T styleData)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r =>
                r.SelectorText == selector ||
                r.SelectorText == selector.Replace(" > ", ">"));

            if (rule == null)
            {
                stylesheet.Insert($"{selector} {{}}", stylesheet.Rules.Length);
                rule = stylesheet.Rules.LastOrDefault() as ICssStyleRule;
            }

            if (rule != null)
            {
                setProperties(rule, styleData);
            }
        }
    }
}
