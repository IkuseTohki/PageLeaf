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
        public CssStyleInfo ParseCss(string cssContent)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(cssContent);

            var styleInfo = new CssStyleInfo();

            ParseBodyStyles(stylesheet, styleInfo);
            ParseParagraphStyles(stylesheet, styleInfo);
            ParseTitleStyles(stylesheet, styleInfo);
            ParseHeadingStyles(stylesheet, styleInfo);
            ParseBlockquoteStyles(stylesheet, styleInfo);
            ParseListStyles(stylesheet, styleInfo);
            ParseTableStyles(stylesheet, styleInfo);
            ParseCodeStyles(stylesheet, styleInfo);
            ParseNumberingStates(stylesheet, styleInfo);
            ParseFootnoteStyles(stylesheet, styleInfo);

            return styleInfo;
        }

        public string UpdateCssContent(string existingCss, CssStyleInfo styleInfo)
        {
            if (styleInfo == null) throw new ArgumentNullException(nameof(styleInfo));

            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(existingCss);

            // Body
            UpdateOrCreateRule(stylesheet, "body", (rule, info) =>
            {
                // 旧プロパティからモデルへの反映（ViewModelからの入力を受け取るため）
                info.Body.TextColor = !string.IsNullOrEmpty(info.BodyTextColor) ? CssColor.Parse(info.BodyTextColor) : null;
                info.Body.BackgroundColor = !string.IsNullOrEmpty(info.BodyBackgroundColor) ? CssColor.Parse(info.BodyBackgroundColor) : null;
                info.Body.FontSize = !string.IsNullOrEmpty(info.BodyFontSize) ? CssSize.Parse(info.BodyFontSize) : null;

                info.Body.ApplyTo(rule);
            }, styleInfo);

            // Paragraph
            UpdateOrCreateRule(stylesheet, "p", (rule, info) =>
            {
                // 旧プロパティからモデルへの同期
                info.Paragraph.LineHeight = info.ParagraphLineHeight;
                info.Paragraph.MarginBottom = !string.IsNullOrEmpty(info.ParagraphMarginBottom) ? CssSize.Parse(info.ParagraphMarginBottom) : null;
                info.Paragraph.TextIndent = !string.IsNullOrEmpty(info.ParagraphTextIndent) ? CssSize.Parse(info.ParagraphTextIndent) : null;

                info.Paragraph.ApplyTo(rule);
            }, styleInfo);

            // Title
            UpdateOrCreateRule(stylesheet, "#page-title", (rule, info) =>
            {
                // 旧プロパティからモデルへの同期
                info.Title.TextColor = !string.IsNullOrEmpty(info.TitleTextColor) ? CssColor.Parse(info.TitleTextColor) : null;
                info.Title.FontSize = !string.IsNullOrEmpty(info.TitleFontSize) ? CssSize.Parse(info.TitleFontSize) : null;
                info.Title.FontFamily = info.TitleFontFamily;
                info.Title.TextAlignment = info.TitleAlignment;
                info.Title.MarginBottom = !string.IsNullOrEmpty(info.TitleMarginBottom) ? CssSize.Parse(info.TitleMarginBottom) : null;
                info.Title.IsBold = info.TitleStyleFlags.IsBold;

                // Note: Italic/Underline are handled via CssTextStyle inside TitleStyle
                info.Title.TextStyle.IsItalic = info.TitleStyleFlags.IsItalic;
                info.Title.TextStyle.IsUnderline = info.TitleStyleFlags.IsUnderline;

                info.Title.ApplyTo(rule);
            }, styleInfo);

            // Headings
            foreach (var level in new[] { "h1", "h2", "h3", "h4", "h5", "h6" })
            {
                UpdateOrCreateRule(stylesheet, level, (rule, info) =>
                {
                    var headingStyle = info.Headings[level];

                    // 旧プロパティ(Dictionary)からモデルへの同期
                    if (info.HeadingTextColors.TryGetValue(level, out var color))
                        headingStyle.TextColor = !string.IsNullOrEmpty(color) ? CssColor.Parse(color) : null;
                    if (info.HeadingFontSizes.TryGetValue(level, out var size))
                        headingStyle.FontSize = !string.IsNullOrEmpty(size) ? CssSize.Parse(size) : null;
                    if (info.HeadingFontFamilies.TryGetValue(level, out var family))
                        headingStyle.FontFamily = family;
                    if (info.HeadingAlignments.TryGetValue(level, out var align))
                        headingStyle.TextAlignment = align;
                    if (info.HeadingStyleFlags.TryGetValue(level, out var flags))
                    {
                        headingStyle.IsBold = flags.IsBold;
                        headingStyle.IsItalic = flags.IsItalic;
                        headingStyle.IsUnderline = flags.IsUnderline;
                    }

                    headingStyle.ApplyTo(rule);
                }, styleInfo);
            }

            // Blockquote
            UpdateOrCreateRule(stylesheet, "blockquote", (rule, info) =>
            {
                // 旧プロパティからモデルへの同期
                info.Blockquote.TextColor = !string.IsNullOrEmpty(info.QuoteTextColor) ? CssColor.Parse(info.QuoteTextColor!) : null;
                info.Blockquote.BackgroundColor = !string.IsNullOrEmpty(info.QuoteBackgroundColor) ? CssColor.Parse(info.QuoteBackgroundColor!) : null;
                info.Blockquote.BorderWidth = info.QuoteBorderWidth;
                info.Blockquote.BorderStyle = info.QuoteBorderStyle;
                info.Blockquote.BorderColor = !string.IsNullOrEmpty(info.QuoteBorderColor) ? CssColor.Parse(info.QuoteBorderColor!) : null;

                info.Blockquote.ApplyTo(rule);
            }, styleInfo);

            // List
            UpdateOrCreateRule(stylesheet, "ul", (rule, info) =>
            {
                info.List.UnorderedListMarkerType = info.ListMarkerType;
                info.List.OrderedListMarkerType = info.NumberedListMarkerType;
                info.List.ListIndent = !string.IsNullOrEmpty(info.ListIndent) ? CssSize.Parse(info.ListIndent) : null;
                info.List.MarkerSize = !string.IsNullOrEmpty(info.ListMarkerSize) ? CssSize.Parse(info.ListMarkerSize) : null;

                info.List.ApplyTo(stylesheet);
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "ol", (rule, info) =>
            {
                // Note: ListIndent application to ol/ul is handled within ListStyle.ApplyTo
            }, styleInfo);

            // Cleanup legacy generic selectors if they exist when switching back to standard
            UpdateOrCreateRule(stylesheet, "li", (rule, info) =>
            {
                if (info.NumberedListMarkerType != "decimal-nested")
                {
                    rule.Style.RemoveProperty("display");
                }
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "li::before", (rule, info) =>
            {
                if (info.NumberedListMarkerType != "decimal-nested")
                {
                    rule.Style.RemoveProperty("content");
                    rule.Style.RemoveProperty("counter-increment");
                }
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "ol > li", (rule, info) =>
            {
                if (info.NumberedListMarkerType == "decimal-nested")
                {
                    rule.Style.SetProperty("display", "block");
                }
                else
                {
                    rule.Style.RemoveProperty("display");
                }
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "ol > li::before", (rule, info) =>
            {
                if (info.NumberedListMarkerType == "decimal-nested")
                {
                    rule.Style.SetProperty("content", "counters(item, \".\") \" \"");
                    rule.Style.SetProperty("counter-increment", "item");
                }
                else
                {
                    rule.Style.RemoveProperty("content");
                    rule.Style.RemoveProperty("counter-increment");
                }
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "li::marker", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.ListMarkerSize)) rule.Style.SetProperty("font-size", info.ListMarkerSize);
            }, styleInfo);

            // チェックボックス（タスクリスト）の場合はマーカーを消す
            UpdateOrCreateRule(stylesheet, "li:has(input[type=\"checkbox\"])", (rule, info) =>
            {
                rule.Style.SetProperty("list-style-type", "none");
            }, styleInfo);

            // Table
            UpdateOrCreateRule(stylesheet, "table", (rule, info) =>
            {
                // 同期
                info.Table.BorderWidth = info.TableBorderWidth;
                info.Table.BorderColor = !string.IsNullOrEmpty(info.TableBorderColor) ? CssColor.Parse(info.TableBorderColor!) : null;
                info.Table.BorderStyle = info.TableBorderStyle;
                info.Table.CellPadding = info.TableCellPadding;
                info.Table.HeaderBackgroundColor = !string.IsNullOrEmpty(info.TableHeaderBackgroundColor) ? CssColor.Parse(info.TableHeaderBackgroundColor!) : null;
                info.Table.HeaderTextColor = !string.IsNullOrEmpty(info.TableHeaderTextColor) ? CssColor.Parse(info.TableHeaderTextColor!) : null;
                info.Table.HeaderFontSize = info.TableHeaderFontSize;
                info.Table.HeaderAlignment = info.TableHeaderAlignment;

                info.Table.ApplyTo(stylesheet);
            }, styleInfo);

            // Code
            UpdateOrCreateRule(stylesheet, "code", (rule, info) =>
            {
                // 同期
                info.Code.TextColor = !string.IsNullOrEmpty(info.InlineCodeTextColor) ? CssColor.Parse(info.InlineCodeTextColor!) :
                                     (!string.IsNullOrEmpty(info.CodeTextColor) ? CssColor.Parse(info.CodeTextColor!) : null);
                info.Code.BackgroundColor = !string.IsNullOrEmpty(info.InlineCodeBackgroundColor) ? CssColor.Parse(info.InlineCodeBackgroundColor!) :
                                           (!string.IsNullOrEmpty(info.CodeBackgroundColor) ? CssColor.Parse(info.CodeBackgroundColor!) : null);
                info.Code.FontFamily = info.CodeFontFamily;
                info.Code.BlockTextColor = !string.IsNullOrEmpty(info.BlockCodeTextColor) ? CssColor.Parse(info.BlockCodeTextColor!) : null;
                info.Code.BlockBackgroundColor = !string.IsNullOrEmpty(info.BlockCodeBackgroundColor) ? CssColor.Parse(info.BlockCodeBackgroundColor!) : null;
                info.Code.IsBlockOverrideEnabled = info.IsCodeBlockOverrideEnabled;

                info.Code.ApplyTo(stylesheet);
            }, styleInfo);

            UpdateHeadingNumbering(stylesheet, styleInfo);

            // Footnotes
            UpdateOrCreateRule(stylesheet, ".footnote-ref", (rule, info) =>
            {
                // 同期 (ViewModelから流れてくる旧プロパティをモデルへ反映)
                info.Footnote.MarkerTextColor = !string.IsNullOrEmpty(info.FootnoteMarkerTextColor) ? CssColor.Parse(info.FootnoteMarkerTextColor!) : null;
                info.Footnote.AreaFontSize = !string.IsNullOrEmpty(info.FootnoteAreaFontSize) ? CssSize.Parse(info.FootnoteAreaFontSize!) : null;
                info.Footnote.AreaTextColor = !string.IsNullOrEmpty(info.FootnoteAreaTextColor) ? CssColor.Parse(info.FootnoteAreaTextColor!) : null;
                info.Footnote.AreaMarginTop = !string.IsNullOrEmpty(info.FootnoteAreaMarginTop) ? CssSize.Parse(info.FootnoteAreaMarginTop!) : null;
                info.Footnote.AreaBorderTopWidth = !string.IsNullOrEmpty(info.FootnoteAreaBorderTopWidth) ? CssSize.Parse(info.FootnoteAreaBorderTopWidth!) : null;
                info.Footnote.AreaBorderTopColor = !string.IsNullOrEmpty(info.FootnoteAreaBorderTopColor) ? CssColor.Parse(info.FootnoteAreaBorderTopColor!) : null;
                info.Footnote.AreaBorderTopStyle = info.FootnoteAreaBorderTopStyle;
                info.Footnote.ListItemLineHeight = info.FootnoteListItemLineHeight;

                info.Footnote.ApplyTo(stylesheet);
            }, styleInfo);

            // 更新されたスタイルシートを文字列として出力
            string generatedCss;
            using (var writer = new StringWriter())
            {
                stylesheet.ToCss(writer, new PageLeaf.Utilities.PrettyStyleFormatter()); // PrettyStyleFormatter を使用
                generatedCss = writer.ToString();
            }

            // 後処理で th, td スタイルをショートハンドに置換
            var thTdBlockPattern = @"th,\s*td\s*\{[^\}]+\}";
            var match = Regex.Match(generatedCss, thTdBlockPattern, RegexOptions.Singleline);

            if (match.Success)
            {
                var newStyles = new List<string>();
                bool hasBorder = !string.IsNullOrEmpty(styleInfo?.TableBorderWidth) || !string.IsNullOrEmpty(styleInfo?.TableBorderColor) || !string.IsNullOrEmpty(styleInfo?.TableBorderStyle);
                bool hasPadding = !string.IsNullOrEmpty(styleInfo?.TableCellPadding);

                if (hasBorder)
                {
                    var borderWidth = !string.IsNullOrEmpty(styleInfo?.TableBorderWidth) ? styleInfo!.TableBorderWidth : "1px";
                    var borderStyle = !string.IsNullOrEmpty(styleInfo?.TableBorderStyle) ? styleInfo!.TableBorderStyle : "solid";
                    var borderColor = !string.IsNullOrEmpty(styleInfo?.TableBorderColor) ? styleInfo!.TableBorderColor : "black";
                    newStyles.Add($"  border: {borderWidth} {borderStyle} {borderColor};");
                }

                if (hasPadding)
                {
                    newStyles.Add($"  padding: {styleInfo?.TableCellPadding};");
                }

                if (newStyles.Any())
                {
                    var newBlockContent = string.Join(Environment.NewLine, newStyles);
                    var newBlock = $"th, td {{{Environment.NewLine}{newBlockContent}{Environment.NewLine}}}";
                    generatedCss = Regex.Replace(generatedCss, thTdBlockPattern, newBlock);
                }
            }

            // Fix for AngleSharp malformed counters syntax
            // AngleSharp normalizes 'counters(item, ".")' to 'counters(item .)' which is invalid CSS.
            generatedCss = generatedCss.Replace("counters(item .)", "counters(item, \".\")");

            // Final cleanup: remove empty rules (e.g. "h4 {}")
            generatedCss = Regex.Replace(generatedCss, @"[^\r\n\}]+\s*\{\s*\}", "");
            // Remove excessive newlines caused by empty rule removal
            generatedCss = Regex.Replace(generatedCss, @"(\r?\n){3,}", Environment.NewLine + Environment.NewLine);

            return generatedCss.Trim();
        }

        private void UpdateHeadingNumbering(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            if (stylesheet?.Rules == null || styleInfo == null) return;

            ClearExistingNumberingRules(stylesheet);
            UpdateBodyCounterReset(stylesheet, styleInfo);
            UpdateHeadingCounterRules(stylesheet, styleInfo);
            UpdateHeadingBeforeRules(stylesheet, styleInfo);
        }

        private void UpdateHeadingCounterRules(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                UpdateOrCreateRule(stylesheet, selector, (rule, info) =>
                {
                    bool isEnabled = info.HeadingNumberingStates != null &&
                                   info.HeadingNumberingStates.TryGetValue(selector, out bool val) && val;

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
                }, styleInfo);
            }
        }

        private void UpdateHeadingBeforeRules(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                bool isEnabled = styleInfo.HeadingNumberingStates != null &&
                               styleInfo.HeadingNumberingStates.TryGetValue(selector, out bool val) && val;

                if (isEnabled)
                {
                    UpdateOrCreateRule(stylesheet, $"{selector}::before", (rule, info) =>
                    {
                        rule.Style.SetProperty("content", BuildCounterContent(i, info));
                    }, styleInfo);
                }
            }
        }

        private string BuildCounterContent(int level, CssStyleInfo info)
        {
            var sb = new StringBuilder();
            bool isFirst = true;
            for (int j = 1; j <= level; j++)
            {
                var h = $"h{j}";
                if (info.HeadingNumberingStates != null && info.HeadingNumberingStates.TryGetValue(h, out bool val) && val)
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

        private void UpdateBodyCounterReset(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            bool anyEnabled = styleInfo.HeadingNumberingStates != null &&
                             styleInfo.HeadingNumberingStates.Any(kvp => kvp.Value);

            UpdateOrCreateRule(stylesheet, "body", (rule, info) =>
            {
                if (anyEnabled)
                {
                    rule.Style.SetProperty("counter-reset", "h1 0");
                }
                else
                {
                    rule.Style.RemoveProperty("counter-reset");
                }
            }, styleInfo);
        }


        private void UpdateOrCreateRule(ICssStyleSheet stylesheet, string selector, Action<ICssStyleRule, CssStyleInfo> setProperties, CssStyleInfo styleInfo)
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
                setProperties(rule, styleInfo);
            }
        }


        private void ParseBodyStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "body");
            if (rule != null)
            {
                styleInfo.Body.UpdateFrom(rule);

                // 旧プロパティとの同期（既存のViewModel/Viewを壊さないため）
                styleInfo.BodyTextColor = styleInfo.Body.TextColor?.ToString();
                styleInfo.BodyBackgroundColor = styleInfo.Body.BackgroundColor?.ToString();
                styleInfo.BodyFontSize = styleInfo.Body.FontSize?.ToString();
            }
        }

        private void ParseParagraphStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "p");
            if (rule != null)
            {
                styleInfo.Paragraph.UpdateFrom(rule);

                // 旧プロパティとの同期
                styleInfo.ParagraphLineHeight = styleInfo.Paragraph.LineHeight;
                styleInfo.ParagraphMarginBottom = styleInfo.Paragraph.MarginBottom?.ToString();
                styleInfo.ParagraphTextIndent = styleInfo.Paragraph.TextIndent?.ToString();
            }
        }

        private void ParseTitleStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "#page-title");
            if (rule != null)
            {
                styleInfo.Title.UpdateFrom(rule);

                // 旧プロパティとの同期
                styleInfo.TitleTextColor = styleInfo.Title.TextColor?.ToString();
                styleInfo.TitleFontSize = styleInfo.Title.FontSize?.ToString();
                styleInfo.TitleFontFamily = styleInfo.Title.FontFamily;
                styleInfo.TitleAlignment = styleInfo.Title.TextAlignment;
                styleInfo.TitleMarginBottom = styleInfo.Title.MarginBottom?.ToString();
                styleInfo.TitleStyleFlags.IsBold = styleInfo.Title.IsBold;

                // Note: IsItalic/IsUnderline are now managed via CssTextStyle in TitleStyle
                styleInfo.TitleStyleFlags.IsItalic = styleInfo.Title.TextStyle.IsItalic;
                styleInfo.TitleStyleFlags.IsUnderline = styleInfo.Title.TextStyle.IsUnderline;
            }
        }

        private void ParseHeadingStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            foreach (var level in new[] { "h1", "h2", "h3", "h4", "h5", "h6" })
            {
                var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == level);
                if (rule != null)
                {
                    var headingStyle = styleInfo.Headings[level];
                    headingStyle.UpdateFrom(rule);

                    // 旧プロパティ(Dictionary)との同期
                    styleInfo.HeadingTextColors[level] = headingStyle.TextColor?.ToString();
                    styleInfo.HeadingFontSizes[level] = headingStyle.FontSize?.ToString();
                    styleInfo.HeadingFontFamilies[level] = headingStyle.FontFamily;
                    styleInfo.HeadingAlignments[level] = headingStyle.TextAlignment;
                    styleInfo.HeadingStyleFlags[level] = new HeadingStyleFlags
                    {
                        IsBold = headingStyle.IsBold,
                        IsItalic = headingStyle.IsItalic,
                        IsUnderline = headingStyle.IsUnderline
                    };
                }
            }
        }

        private void ParseBlockquoteStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "blockquote");
            if (rule != null)
            {
                styleInfo.Blockquote.UpdateFrom(rule);

                // 旧プロパティとの同期
                styleInfo.QuoteTextColor = styleInfo.Blockquote.TextColor?.ToString();
                styleInfo.QuoteBackgroundColor = styleInfo.Blockquote.BackgroundColor?.ToString();
                styleInfo.QuoteBorderWidth = styleInfo.Blockquote.BorderWidth;
                styleInfo.QuoteBorderStyle = styleInfo.Blockquote.BorderStyle;
                styleInfo.QuoteBorderColor = styleInfo.Blockquote.BorderColor?.ToString();
            }
        }

        private void ParseListStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            styleInfo.List.UpdateFrom(stylesheet);

            // 旧プロパティとの同期
            styleInfo.ListMarkerType = styleInfo.List.UnorderedListMarkerType;
            styleInfo.NumberedListMarkerType = styleInfo.List.OrderedListMarkerType;
            styleInfo.ListIndent = styleInfo.List.ListIndent?.ToString();
            styleInfo.ListMarkerSize = styleInfo.List.MarkerSize?.ToString();
        }

        private void ParseTableStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            styleInfo.Table.UpdateFrom(stylesheet);

            // 旧プロパティとの同期
            styleInfo.TableBorderWidth = styleInfo.Table.BorderWidth;
            styleInfo.TableBorderColor = styleInfo.Table.BorderColor?.ToString();
            styleInfo.TableBorderStyle = styleInfo.Table.BorderStyle;
            styleInfo.TableCellPadding = styleInfo.Table.CellPadding;
            styleInfo.TableHeaderBackgroundColor = styleInfo.Table.HeaderBackgroundColor?.ToString();
            styleInfo.TableHeaderTextColor = styleInfo.Table.HeaderTextColor?.ToString();
            styleInfo.TableHeaderFontSize = styleInfo.Table.HeaderFontSize;
            styleInfo.TableHeaderAlignment = styleInfo.Table.HeaderAlignment;
        }

        private void ParseCodeStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            styleInfo.Code.UpdateFrom(stylesheet);

            // 旧プロパティとの同期
            styleInfo.CodeTextColor = styleInfo.Code.TextColor?.ToString();
            styleInfo.CodeBackgroundColor = styleInfo.Code.BackgroundColor?.ToString();
            styleInfo.InlineCodeTextColor = styleInfo.Code.TextColor?.ToString();
            styleInfo.InlineCodeBackgroundColor = styleInfo.Code.BackgroundColor?.ToString();
            styleInfo.CodeFontFamily = styleInfo.Code.FontFamily;
            styleInfo.BlockCodeTextColor = styleInfo.Code.BlockTextColor?.ToString();
            styleInfo.BlockCodeBackgroundColor = styleInfo.Code.BlockBackgroundColor?.ToString();
            styleInfo.IsCodeBlockOverrideEnabled = styleInfo.Code.IsBlockOverrideEnabled;
        }

        private void ParseNumberingStates(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
                var beforeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == $"{selector}::before");

                if (rule?.Style.GetPropertyValue("counter-increment")?.Contains(selector) == true &&
                    beforeRule?.Style.GetPropertyValue("content")?.Contains($"counter({selector})") == true)
                {
                    styleInfo.HeadingNumberingStates[selector] = true;
                }
                else
                {
                    styleInfo.HeadingNumberingStates[selector] = false;
                }
            }
        }

        private void ParseFootnoteStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            styleInfo.Footnote.UpdateFrom(stylesheet);

            // 旧プロパティとの同期（既存ロジック・テスト用）
            styleInfo.FootnoteMarkerTextColor = styleInfo.Footnote.MarkerTextColor?.ToString();
            styleInfo.FootnoteAreaFontSize = styleInfo.Footnote.AreaFontSize?.ToString();
            styleInfo.FootnoteAreaTextColor = styleInfo.Footnote.AreaTextColor?.ToString();
            styleInfo.FootnoteAreaMarginTop = styleInfo.Footnote.AreaMarginTop?.ToString();
            styleInfo.FootnoteAreaBorderTopWidth = styleInfo.Footnote.AreaBorderTopWidth?.ToString();
            styleInfo.FootnoteAreaBorderTopColor = styleInfo.Footnote.AreaBorderTopColor?.ToString();
            styleInfo.FootnoteAreaBorderTopStyle = styleInfo.Footnote.AreaBorderTopStyle;
            styleInfo.FootnoteListItemLineHeight = styleInfo.Footnote.ListItemLineHeight;
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
                // System.Windows.Media.ColorConverter.ConvertFromString は "rgb(r, g, b)" や "rgba(r, g, b, a)" 形式をパースできる
                var color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(colorValue);
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
    }
}
