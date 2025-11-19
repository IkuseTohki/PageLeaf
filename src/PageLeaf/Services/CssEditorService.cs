using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using PageLeaf.Models;
using System.Linq;
using System.Text;
using System.IO;
using PageLeaf.Utilities;
using System.Windows.Media; // ColorConverter を使用するために追加
using AngleSharp.Css.Values; // AngleSharp.Css.Values.Color を使用するために追加
using System.Collections.Generic; // List<string> を使用するために追加

namespace PageLeaf.Services
{
    public class CssEditorService : ICssEditorService
    {
        public CssStyleInfo ParseCss(string cssContent)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(cssContent);

            var styleInfo = new CssStyleInfo();

            var bodyRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "body");

            if (bodyRule != null)
            {
                styleInfo.BodyTextColor = GetColorHexFromRule(bodyRule, "color");
                styleInfo.BodyBackgroundColor = GetColorHexFromRule(bodyRule, "background-color");

                // font-size プロパティを読み取る
                styleInfo.BodyFontSize = bodyRule.Style.GetPropertyValue("font-size");
            }

            // h1からh6までの見出しのcolorプロパティを解析
            for (int i = 1; i <= 6; i++)
            {
                var headingSelector = $"h{i}";
                var headingRule = stylesheet.Rules
                    .OfType<ICssStyleRule>()
                    .FirstOrDefault(r => r.SelectorText == headingSelector);

                if (headingRule != null)
                {
                    var colorProperty = headingRule.Style.GetProperty("color");
                    if (colorProperty != null && colorProperty.RawValue is AngleSharp.Css.Values.Color angleSharpColor)
                    {
                        // AngleSharpは色をrgba形式で正規化するため、CssTextプロパティを使用してそのまま格納
                        styleInfo.HeadingTextColors[headingSelector] = angleSharpColor.CssText;
                    }

                    // font-sizeプロパティを解析
                    var fontSize = headingRule.Style.GetPropertyValue("font-size");
                    if (!string.IsNullOrEmpty(fontSize))
                    {
                        styleInfo.HeadingFontSizes[headingSelector] = fontSize;
                    }

                    // font-familyプロパティを解析
                    var fontFamily = headingRule.Style.GetPropertyValue("font-family");
                    if (!string.IsNullOrEmpty(fontFamily))
                    {
                        styleInfo.HeadingFontFamilies[headingSelector] = fontFamily;
                    }

                    // スタイルフラグを解析
                    var flags = new HeadingStyleFlags();
                    var fontWeight = headingRule.Style.GetPropertyValue("font-weight");
                    if (fontWeight == "bold" || (int.TryParse(fontWeight, out var weight) && weight >= 700))
                    {
                        flags.IsBold = true;
                    }

                    var fontStyle = headingRule.Style.GetPropertyValue("font-style");
                    if (fontStyle == "italic")
                    {
                        flags.IsItalic = true;
                    }

                    var textDecoration = headingRule.Style.GetPropertyValue("text-decoration");
                    if (textDecoration.Contains("underline"))
                    {
                        flags.IsUnderline = true;
                    }
                    if (textDecoration.Contains("line-through"))
                    {
                        flags.IsStrikethrough = true;
                    }
                    styleInfo.HeadingStyleFlags[headingSelector] = flags;
                }
            }

            // blockquote のスタイルを解析
            var blockquoteRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "blockquote");

            if (blockquoteRule != null)
            {
                styleInfo.QuoteTextColor = GetColorHexFromRule(blockquoteRule, "color");
                styleInfo.QuoteBackgroundColor = GetColorHexFromRule(blockquoteRule, "background-color");

                // border-left
                styleInfo.QuoteBorderWidth = blockquoteRule.Style.GetPropertyValue("border-left-width");
                styleInfo.QuoteBorderStyle = blockquoteRule.Style.GetPropertyValue("border-left-style");
                styleInfo.QuoteBorderColor = GetColorHexFromRule(blockquoteRule, "border-left-color");
            }

            // ul のスタイルを解析
            var ulRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "ul");

            if (ulRule != null)
            {
                styleInfo.ListMarkerType = ulRule.Style.GetPropertyValue("list-style-type");
                styleInfo.ListIndent = ulRule.Style.GetPropertyValue("padding-left");
            }

            // table のスタイルを解析
            var thTdRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "th, td");

            if (thTdRule != null)
            {
                // border
                styleInfo.TableBorderWidth = thTdRule.Style.GetPropertyValue("border-width");
                styleInfo.TableBorderColor = GetColorHexFromRule(thTdRule, "border-color");

                // padding
                styleInfo.TableCellPadding = thTdRule.Style.GetPropertyValue("padding");
            }

            var thRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "th");

            if (thRule != null)
            {
                styleInfo.TableHeaderBackgroundColor = GetColorHexFromRule(thRule, "background-color");
            }

            // code のスタイルを解析
            var codeRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "code");

            if (codeRule != null)
            {
                styleInfo.CodeTextColor = GetColorHexFromRule(codeRule, "color");
                styleInfo.CodeBackgroundColor = GetColorHexFromRule(codeRule, "background-color");
                styleInfo.CodeFontFamily = codeRule.Style.GetPropertyValue("font-family");
            }

            return styleInfo;
        }

        public string UpdateCssContent(string existingCss, CssStyleInfo styleInfo)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(existingCss);

            var bodyRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "body");

            // bodyセレクタが存在しない場合は追加
            if (bodyRule == null)
            {
                // 新しいルールを作成し、スタイルシートの末尾に追加
                var newBodyRuleText = "body {}"; // ルール文字列を直接渡す
                stylesheet.Insert(newBodyRuleText, stylesheet.Rules.Length);
                // Insert されたルールを取得する必要がある
                bodyRule = stylesheet.Rules.LastOrDefault() as ICssStyleRule; // 最後に追加されたルールを取得
            }

            if (bodyRule != null)
            {
                if (!string.IsNullOrEmpty(styleInfo.BodyTextColor))
                {
                    bodyRule.Style.SetProperty("color", styleInfo.BodyTextColor);
                }
                if (!string.IsNullOrEmpty(styleInfo.BodyBackgroundColor))
                {
                    bodyRule.Style.SetProperty("background-color", styleInfo.BodyBackgroundColor);
                }
                if (!string.IsNullOrEmpty(styleInfo.BodyFontSize))
                {
                    bodyRule.Style.SetProperty("font-size", styleInfo.BodyFontSize);
                }
            }

            // h1からh6までの見出しスタイルを更新
            for (int i = 1; i <= 6; i++)
            {
                var headingSelector = $"h{i}";
                var headingRule = stylesheet.Rules
                    .OfType<ICssStyleRule>()
                    .FirstOrDefault(r => r.SelectorText == headingSelector);

                // 見出しセレクタが存在しない場合は追加
                if (headingRule == null)
                {
                    var newHeadingRuleText = $"{headingSelector} {{}}";
                    stylesheet.Insert(newHeadingRuleText, stylesheet.Rules.Length);
                    headingRule = stylesheet.Rules.LastOrDefault() as ICssStyleRule;
                }

                if (headingRule != null)
                {
                    // Color
                    if (styleInfo.HeadingTextColors.TryGetValue(headingSelector, out var color) && !string.IsNullOrEmpty(color))
                    {
                        headingRule.Style.SetProperty("color", color);
                    }
                    else if (styleInfo.HeadingTextColors.ContainsKey(headingSelector) && string.IsNullOrEmpty(color))
                    {
                        headingRule.Style.RemoveProperty("color");
                    }

                    // Font Size
                    if (styleInfo.HeadingFontSizes.TryGetValue(headingSelector, out var fontSize) && !string.IsNullOrEmpty(fontSize))
                    {
                        headingRule.Style.SetProperty("font-size", fontSize);
                    }
                    else if (styleInfo.HeadingFontSizes.ContainsKey(headingSelector) && string.IsNullOrEmpty(fontSize))
                    {
                        headingRule.Style.RemoveProperty("font-size");
                    }

                    // Font Family
                    if (styleInfo.HeadingFontFamilies.TryGetValue(headingSelector, out var fontFamily) && !string.IsNullOrEmpty(fontFamily))
                    {
                        headingRule.Style.SetProperty("font-family", fontFamily);
                    }
                    else if (styleInfo.HeadingFontFamilies.ContainsKey(headingSelector) && string.IsNullOrEmpty(fontFamily))
                    {
                        headingRule.Style.RemoveProperty("font-family");
                    }

                    // Style Flags (Bold, Italic, Underline, Strikethrough)
                    if (styleInfo.HeadingStyleFlags.TryGetValue(headingSelector, out var flags))
                    {
                        // Font Weight
                        if (flags.IsBold)
                        {
                            headingRule.Style.SetProperty("font-weight", "bold");
                        }
                        else
                        {
                            headingRule.Style.SetProperty("font-weight", "normal");
                        }

                        // Font Style
                        if (flags.IsItalic)
                        {
                            headingRule.Style.SetProperty("font-style", "italic");
                        }
                        else
                        {
                            headingRule.Style.SetProperty("font-style", "normal");
                        }

                        // Text Decoration
                        var textDecorations = new List<string>();
                        if (flags.IsUnderline)
                        {
                            textDecorations.Add("underline");
                        }
                        if (flags.IsStrikethrough)
                        {
                            textDecorations.Add("line-through");
                        }

                        if (textDecorations.Any())
                        {
                            headingRule.Style.SetProperty("text-decoration", string.Join(" ", textDecorations));
                            // text-decoration-color を明示的に設定
                            if (styleInfo.HeadingTextColors.TryGetValue(headingSelector, out var textColor) && !string.IsNullOrEmpty(textColor))
                            {
                                headingRule.Style.SetProperty("text-decoration-color", textColor);
                            }
                            else
                            {
                                // 文字色が設定されていない場合は、text-decoration-colorも削除
                                headingRule.Style.RemoveProperty("text-decoration-color");
                            }
                        }
                        else
                        {
                            headingRule.Style.SetProperty("text-decoration", "none");
                            headingRule.Style.RemoveProperty("text-decoration-color"); // 不要な場合は削除
                        }
                    }
                }
            }

            // blockquote スタイルを更新
            var blockquoteRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "blockquote");

            if (blockquoteRule == null)
            {
                var newRuleText = "blockquote {}";
                stylesheet.Insert(newRuleText, stylesheet.Rules.Length);
                blockquoteRule = stylesheet.Rules.LastOrDefault() as ICssStyleRule;
            }

            if (blockquoteRule != null)
            {
                if (!string.IsNullOrEmpty(styleInfo.QuoteTextColor))
                {
                    blockquoteRule.Style.SetProperty("color", styleInfo.QuoteTextColor);
                }
                if (!string.IsNullOrEmpty(styleInfo.QuoteBackgroundColor))
                {
                    blockquoteRule.Style.SetProperty("background-color", styleInfo.QuoteBackgroundColor);
                }

                // border-left プロパティを組み立てる
                if (!string.IsNullOrEmpty(styleInfo.QuoteBorderWidth) ||
                    !string.IsNullOrEmpty(styleInfo.QuoteBorderStyle) ||
                    !string.IsNullOrEmpty(styleInfo.QuoteBorderColor))
                {
                    var borderWidth = !string.IsNullOrEmpty(styleInfo.QuoteBorderWidth) ? styleInfo.QuoteBorderWidth : "medium";
                    var borderStyle = !string.IsNullOrEmpty(styleInfo.QuoteBorderStyle) ? styleInfo.QuoteBorderStyle : "none";
                    var borderColor = !string.IsNullOrEmpty(styleInfo.QuoteBorderColor) ? styleInfo.QuoteBorderColor : "currentcolor";
                    blockquoteRule.Style.SetProperty("border-left", $"{borderWidth} {borderStyle} {borderColor}");
                }
            }

            // ul スタイルを更新
            var ulRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "ul");

            if (ulRule == null)
            {
                var newRuleText = "ul {}";
                stylesheet.Insert(newRuleText, stylesheet.Rules.Length);
                ulRule = stylesheet.Rules.LastOrDefault() as ICssStyleRule;
            }

            if (ulRule != null)
            {
                if (!string.IsNullOrEmpty(styleInfo.ListMarkerType))
                {
                    ulRule.Style.SetProperty("list-style-type", styleInfo.ListMarkerType);
                }
                if (!string.IsNullOrEmpty(styleInfo.ListIndent))
                {
                    ulRule.Style.SetProperty("padding-left", styleInfo.ListIndent);
                }
            }

            // 更新されたスタイルシートを文字列として出力
            using (var writer = new StringWriter())
            {
                stylesheet.ToCss(writer, new PrettyStyleFormatter()); // PrettyStyleFormatter を使用
                return writer.ToString();
            }
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
    }
}
