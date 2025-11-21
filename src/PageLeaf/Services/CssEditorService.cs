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
                    // Color
                    if (info.HeadingTextColors.TryGetValue(headingSelector, out var color) && !string.IsNullOrEmpty(color))
                    {
                        rule.Style.SetProperty("color", color);
                    }
                    else if (info.HeadingTextColors.ContainsKey(headingSelector) && string.IsNullOrEmpty(color))
                    {
                        rule.Style.RemoveProperty("color");
                    }

                    // Font Size
                    if (info.HeadingFontSizes.TryGetValue(headingSelector, out var fontSize) && !string.IsNullOrEmpty(fontSize))
                    {
                        rule.Style.SetProperty("font-size", fontSize);
                    }
                    else if (info.HeadingFontSizes.ContainsKey(headingSelector) && string.IsNullOrEmpty(fontSize))
                    {
                        rule.Style.RemoveProperty("font-size");
                    }

                    // Font Family
                    if (info.HeadingFontFamilies.TryGetValue(headingSelector, out var fontFamily) && !string.IsNullOrEmpty(fontFamily))
                    {
                        rule.Style.SetProperty("font-family", fontFamily);
                    }
                    else if (info.HeadingFontFamilies.ContainsKey(headingSelector) && string.IsNullOrEmpty(fontFamily))
                    {
                        rule.Style.RemoveProperty("font-family");
                    }
                    
                    // Style Flags
                    if (info.HeadingStyleFlags.TryGetValue(headingSelector, out var flags))
                    {
                        rule.Style.SetProperty("font-weight", flags.IsBold ? "bold" : "normal");
                        rule.Style.SetProperty("font-style", flags.IsItalic ? "italic" : "normal");

                        var textDecorations = new List<string>();
                        if (flags.IsUnderline) textDecorations.Add("underline");
                        if (flags.IsStrikethrough) textDecorations.Add("line-through");

                        if (textDecorations.Any())
                        {
                            rule.Style.SetProperty("text-decoration", string.Join(" ", textDecorations));
                            if (info.HeadingTextColors.TryGetValue(headingSelector, out var textColor) && !string.IsNullOrEmpty(textColor))
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

            // Table - 一旦ロングハンドで生成させる
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
            }, styleInfo);

            // Code
            UpdateOrCreateRule(stylesheet, "code", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.CodeTextColor)) rule.Style.SetProperty("color", info.CodeTextColor);
                if (!string.IsNullOrEmpty(info.CodeBackgroundColor)) rule.Style.SetProperty("background-color", info.CodeBackgroundColor);
                if (!string.IsNullOrEmpty(info.CodeFontFamily)) rule.Style.SetProperty("font-family", info.CodeFontFamily);
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
                bool hasBorder = !string.IsNullOrEmpty(styleInfo.TableBorderWidth) || !string.IsNullOrEmpty(styleInfo.TableBorderColor) || !string.IsNullOrEmpty(styleInfo.TableBorderStyle);
                bool hasPadding = !string.IsNullOrEmpty(styleInfo.TableCellPadding);

                if (hasBorder)
                {
                    var borderWidth = !string.IsNullOrEmpty(styleInfo.TableBorderWidth) ? styleInfo.TableBorderWidth : "1px";
                    var borderStyle = !string.IsNullOrEmpty(styleInfo.TableBorderStyle) ? styleInfo.TableBorderStyle : "solid";
                    var borderColor = !string.IsNullOrEmpty(styleInfo.TableBorderColor) ? styleInfo.TableBorderColor : "black";
                    newStyles.Add($"  border: {borderWidth} {borderStyle} {borderColor};");
                }

                if (hasPadding)
                {
                    newStyles.Add($"  padding: {styleInfo.TableCellPadding};");
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
