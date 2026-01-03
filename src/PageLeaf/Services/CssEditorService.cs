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

                    // text-alignプロパティを解析
                    var textAlign = headingRule.Style.GetPropertyValue("text-align");
                    if (!string.IsNullOrEmpty(textAlign))
                    {
                        styleInfo.HeadingAlignments[headingSelector] = textAlign;
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

            // ol のスタイルを解析
            var olRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "ol");

            if (olRule != null)
            {
                styleInfo.NumberedListMarkerType = olRule.Style.GetPropertyValue("list-style-type");
            }

            // li::marker のスタイルを解析
            var markerRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "li::marker");

            if (markerRule != null)
            {
                styleInfo.ListMarkerSize = markerRule.Style.GetPropertyValue("font-size");
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
                styleInfo.TableBorderStyle = thTdRule.Style.GetPropertyValue("border-style");

                // padding
                styleInfo.TableCellPadding = thTdRule.Style.GetPropertyValue("padding");
            }

            var thRule = stylesheet.Rules
                .OfType<ICssStyleRule>()
                .FirstOrDefault(r => r.SelectorText == "th");

            if (thRule != null)
            {
                styleInfo.TableHeaderBackgroundColor = GetColorHexFromRule(thRule, "background-color");
                styleInfo.TableHeaderAlignment = thRule.Style.GetPropertyValue("text-align");
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

            // 項番採番の検出 (見出しレベルごと)
            for (int i = 1; i <= 6; i++)
            {
                var headingSelector = $"h{i}";
                var headingRule = stylesheet.Rules
                    .OfType<ICssStyleRule>()
                    .FirstOrDefault(r => r.SelectorText == headingSelector);

                var beforeRule = stylesheet.Rules
                    .OfType<ICssStyleRule>()
                    .FirstOrDefault(r => r.SelectorText == $"{headingSelector}::before");

                // counter-incrementと::before contentが存在するかで判断
                if (headingRule?.Style.GetPropertyValue("counter-increment")?.Contains(headingSelector) == true &&
                    beforeRule?.Style.GetPropertyValue("content")?.Contains($"counter({headingSelector})") == true)
                {
                    styleInfo.HeadingNumberingStates[headingSelector] = true;
                }
                else
                {
                    styleInfo.HeadingNumberingStates[headingSelector] = false;
                }
            }
            return styleInfo;
        }

        public string UpdateCssContent(string existingCss, CssStyleInfo styleInfo)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet(existingCss);

            // Body
            UpdateOrCreateRule(stylesheet, "body", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.BodyTextColor)) rule.Style.SetProperty("color", info.BodyTextColor);
                if (!string.IsNullOrEmpty(info.BodyBackgroundColor)) rule.Style.SetProperty("background-color", info.BodyBackgroundColor);
                if (!string.IsNullOrEmpty(info.BodyFontSize)) rule.Style.SetProperty("font-size", info.BodyFontSize);
            }, styleInfo);

            // Headings
            for (int i = 1; i <= 6; i++)
            {
                var headingSelector = $"h{i}";
                UpdateOrCreateRule(stylesheet, headingSelector, (rule, info) =>
                {
                    // Helper to simplify property updates
                    void UpdateProperty(Dictionary<string, string?>? dict, string key, string propertyName)
                    {
                        if (dict != null && dict.TryGetValue(key, out var val))
                        {
                            if (!string.IsNullOrEmpty(val)) rule.Style.SetProperty(propertyName, val);
                            else rule.Style.RemoveProperty(propertyName);
                        }
                    }

                    UpdateProperty(info.HeadingTextColors, headingSelector, "color");
                    UpdateProperty(info.HeadingFontSizes, headingSelector, "font-size");
                    UpdateProperty(info.HeadingFontFamilies, headingSelector, "font-family");
                    UpdateProperty(info.HeadingAlignments, headingSelector, "text-align");

                    // Text Align specifically for headings might need override logic in future, but for now standard helper is fine.
                    // Special case: text-align might need !important if we want to override Markdown's default behavior, 
                    // but usually that applies only to table headers.

                    // Style Flags
                    if (info.HeadingStyleFlags != null && info.HeadingStyleFlags.TryGetValue(headingSelector, out var flags) && flags != null)
                    {
                        rule.Style.SetProperty("font-weight", flags.IsBold ? "bold" : "normal");
                        rule.Style.SetProperty("font-style", flags.IsItalic ? "italic" : "normal");

                        var textDecorations = new List<string>();
                        if (flags.IsUnderline) textDecorations.Add("underline");

                        if (textDecorations.Any())
                        {
                            rule.Style.SetProperty("text-decoration", string.Join(" ", textDecorations));
                            if (info.HeadingTextColors != null && info.HeadingTextColors.TryGetValue(headingSelector, out var textColor) && !string.IsNullOrEmpty(textColor))
                            {
                                rule.Style.SetProperty("text-decoration-color", textColor);
                            }
                            else
                            {
                                rule.Style.RemoveProperty("text-decoration-color");
                            }
                        }
                        else
                        {
                            rule.Style.SetProperty("text-decoration", "none");
                            rule.Style.RemoveProperty("text-decoration-color");
                        }
                    }
                }, styleInfo);
            }

            // Blockquote
            UpdateOrCreateRule(stylesheet, "blockquote", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.QuoteTextColor)) rule.Style.SetProperty("color", info.QuoteTextColor);
                if (!string.IsNullOrEmpty(info.QuoteBackgroundColor)) rule.Style.SetProperty("background-color", info.QuoteBackgroundColor);
                if (!string.IsNullOrEmpty(info.QuoteBorderWidth) || !string.IsNullOrEmpty(info.QuoteBorderStyle) || !string.IsNullOrEmpty(info.QuoteBorderColor))
                {
                    var borderWidth = !string.IsNullOrEmpty(info.QuoteBorderWidth) ? info.QuoteBorderWidth : "medium";
                    var borderStyle = !string.IsNullOrEmpty(info.QuoteBorderStyle) ? info.QuoteBorderStyle : "none";
                    var borderColor = !string.IsNullOrEmpty(info.QuoteBorderColor) ? info.QuoteBorderColor : "currentcolor";
                    rule.Style.SetProperty("border-left", $"{borderWidth} {borderStyle} {borderColor}");
                }
            }, styleInfo);

            // List
            UpdateOrCreateRule(stylesheet, "ul", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.ListMarkerType)) rule.Style.SetProperty("list-style-type", info.ListMarkerType);
                if (!string.IsNullOrEmpty(info.ListIndent)) rule.Style.SetProperty("padding-left", info.ListIndent);
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "ol", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.NumberedListMarkerType)) rule.Style.SetProperty("list-style-type", info.NumberedListMarkerType);
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

            // Table - 一旦ロングハンドで生成させる
            UpdateOrCreateRule(stylesheet, "table", (rule, info) =>
            {
                rule.Style.SetProperty("border-collapse", "collapse");
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "th, td", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.TableBorderWidth) || !string.IsNullOrEmpty(info.TableBorderColor) || !string.IsNullOrEmpty(info.TableBorderStyle))
                {
                    var borderWidth = !string.IsNullOrEmpty(info.TableBorderWidth) ? info.TableBorderWidth : "1px";
                    var borderStyle = !string.IsNullOrEmpty(info.TableBorderStyle) ? info.TableBorderStyle : "solid";
                    var borderColor = !string.IsNullOrEmpty(info.TableBorderColor) ? info.TableBorderColor : "black";
                    rule.Style.SetProperty("border", $"{borderWidth} {borderStyle} {borderColor}");
                }
                if (!string.IsNullOrEmpty(info.TableCellPadding)) rule.Style.SetProperty("padding", info.TableCellPadding);
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "th", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.TableHeaderBackgroundColor)) rule.Style.SetProperty("background-color", info.TableHeaderBackgroundColor);
                if (!string.IsNullOrEmpty(info.TableHeaderAlignment)) rule.Style.SetProperty("text-align", info.TableHeaderAlignment, "important");
            }, styleInfo);

            // Code
            UpdateOrCreateRule(stylesheet, "code", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.CodeTextColor)) rule.Style.SetProperty("color", info.CodeTextColor);
                if (!string.IsNullOrEmpty(info.CodeBackgroundColor)) rule.Style.SetProperty("background-color", info.CodeBackgroundColor);
                if (!string.IsNullOrEmpty(info.CodeFontFamily)) rule.Style.SetProperty("font-family", info.CodeFontFamily);
            }, styleInfo);

            if (styleInfo != null)
            {
                UpdateHeadingNumbering(stylesheet, styleInfo);
            }

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

            return generatedCss;
        }

        private void UpdateHeadingNumbering(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            if (stylesheet?.Rules == null || styleInfo == null) return;

            // Step 1: Remove all ::before rules for headings to have a clean slate.
            for (int i = stylesheet.Rules.Length - 1; i >= 0; i--)
            {
                var rule = stylesheet.Rules[i];
                if (rule is ICssStyleRule styleRule && styleRule.SelectorText != null && styleRule.SelectorText.StartsWith("h") && styleRule.SelectorText.Contains("::before"))
                {
                    stylesheet.RemoveAt(i);
                }
            }

            // Check if any heading numbering is enabled at all to decide if body counter-reset is needed
            bool anyHeadingNumberingEnabled = styleInfo.HeadingNumberingStates != null && styleInfo.HeadingNumberingStates.Any(kvp => kvp.Value);

            // Step 2: Update counter properties on body and headings
            UpdateOrCreateRule(stylesheet, "body", (rule, info) =>
            {
                if (anyHeadingNumberingEnabled)
                {
                    rule.Style.SetProperty("counter-reset", "h1 0"); // Reset h1 counter on body
                }
                else
                {
                    rule.Style.RemoveProperty("counter-reset");
                }
            }, styleInfo);

            for (int i = 1; i <= 6; i++)
            {
                var headingSelector = $"h{i}";
                UpdateOrCreateRule(stylesheet, headingSelector, (rule, info) =>
                {
                    if (info.HeadingNumberingStates != null && info.HeadingNumberingStates.TryGetValue(headingSelector, out bool isEnabled) && isEnabled)
                    {
                        rule.Style.SetProperty("counter-increment", headingSelector);
                        // Reset counter for sub-headings if current heading numbering is enabled
                        if (i < 6)
                        {
                            rule.Style.SetProperty("counter-reset", $"h{i + 1} 0");
                        }
                    }
                    else // Numbering is disabled for this specific heading level
                    {
                        rule.Style.RemoveProperty("counter-increment");
                        rule.Style.RemoveProperty("counter-reset");
                    }
                }, styleInfo);
            }

            // Step 3: Add ::before rules if numbering is enabled for a specific heading
            for (int i = 1; i <= 6; i++)
            {
                var headingSelector = $"h{i}";
                if (styleInfo.HeadingNumberingStates != null && styleInfo.HeadingNumberingStates.TryGetValue(headingSelector, out bool isEnabled) && isEnabled)
                {
                    var beforeSelector = $"{headingSelector}::before";
                    UpdateOrCreateRule(stylesheet, beforeSelector, (rule, info) =>
                    {
                        var contentBuilder = new StringBuilder();
                        for (int j = 1; j <= i; j++)
                        {
                            contentBuilder.Append($"counter(h{j})");
                            if (j < i)
                            {
                                contentBuilder.Append(" \".\" ");
                            }
                        }
                        contentBuilder.Append(" \". \"");
                        rule.Style.SetProperty("content", contentBuilder.ToString());
                    }, styleInfo);
                }
            }
        }


        private void UpdateOrCreateRule(ICssStyleSheet stylesheet, string selector, Action<ICssStyleRule, CssStyleInfo> setProperties, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
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
