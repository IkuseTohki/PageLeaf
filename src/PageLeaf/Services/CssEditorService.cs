using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
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
                if (!string.IsNullOrEmpty(info.BodyTextColor)) rule.Style.SetProperty("color", info.BodyTextColor);
                if (!string.IsNullOrEmpty(info.BodyBackgroundColor)) rule.Style.SetProperty("background-color", info.BodyBackgroundColor);
                if (!string.IsNullOrEmpty(info.BodyFontSize)) rule.Style.SetProperty("font-size", info.BodyFontSize);
            }, styleInfo);

            // Paragraph
            UpdateOrCreateRule(stylesheet, "p", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.ParagraphLineHeight)) rule.Style.SetProperty("line-height", info.ParagraphLineHeight);
                if (!string.IsNullOrEmpty(info.ParagraphMarginBottom)) rule.Style.SetProperty("margin-bottom", info.ParagraphMarginBottom);
                if (!string.IsNullOrEmpty(info.ParagraphTextIndent)) rule.Style.SetProperty("text-indent", info.ParagraphTextIndent);
            }, styleInfo);

            // Page Title
            UpdateOrCreateRule(stylesheet, "#page-title", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.TitleTextColor)) rule.Style.SetProperty("color", info.TitleTextColor);
                if (!string.IsNullOrEmpty(info.TitleFontSize)) rule.Style.SetProperty("font-size", info.TitleFontSize);
                if (!string.IsNullOrEmpty(info.TitleFontFamily)) rule.Style.SetProperty("font-family", info.TitleFontFamily);
                if (!string.IsNullOrEmpty(info.TitleAlignment)) rule.Style.SetProperty("text-align", info.TitleAlignment);
                if (!string.IsNullOrEmpty(info.TitleMarginBottom)) rule.Style.SetProperty("margin-bottom", info.TitleMarginBottom);

                if (info.TitleStyleFlags != null)
                {
                    rule.Style.SetProperty("font-weight", info.TitleStyleFlags.IsBold ? "bold" : "normal");
                    rule.Style.SetProperty("font-style", info.TitleStyleFlags.IsItalic ? "italic" : "normal");

                    var textDecorations = new List<string>();
                    if (info.TitleStyleFlags.IsUnderline) textDecorations.Add("underline");

                    if (textDecorations.Any())
                    {
                        rule.Style.SetProperty("text-decoration", string.Join(" ", textDecorations));
                        if (!string.IsNullOrEmpty(info.TitleTextColor))
                        {
                            rule.Style.SetProperty("text-decoration-color", info.TitleTextColor);
                        }
                    }
                    else
                    {
                        rule.Style.SetProperty("text-decoration", "none");
                        rule.Style.RemoveProperty("text-decoration-color");
                    }
                }
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
                if (!string.IsNullOrEmpty(info.ListIndent)) rule.Style.SetProperty("padding-left", info.ListIndent);

                if (info.NumberedListMarkerType == "decimal-nested")
                {
                    rule.Style.SetProperty("list-style-type", "none");
                    rule.Style.SetProperty("counter-reset", "item 0");
                }
                else
                {
                    if (!string.IsNullOrEmpty(info.NumberedListMarkerType))
                    {
                        rule.Style.SetProperty("list-style-type", info.NumberedListMarkerType);
                    }
                    rule.Style.RemoveProperty("counter-reset");
                }
            }, styleInfo);

            // Cleanup legacy generic selectors if they exist
            UpdateOrCreateRule(stylesheet, "li", (rule, info) =>
            {
                rule.Style.RemoveProperty("display");
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "li::before", (rule, info) =>
            {
                rule.Style.RemoveProperty("content");
                rule.Style.RemoveProperty("counter-increment");
            }, styleInfo);

            // Apply nested decimal styles to specific selectors (ol > li)
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
                if (!string.IsNullOrEmpty(info.TableHeaderTextColor)) rule.Style.SetProperty("color", info.TableHeaderTextColor);
                if (!string.IsNullOrEmpty(info.TableHeaderFontSize)) rule.Style.SetProperty("font-size", info.TableHeaderFontSize);
                if (!string.IsNullOrEmpty(info.TableHeaderAlignment)) rule.Style.SetProperty("text-align", info.TableHeaderAlignment, "important");
            }, styleInfo);

            // Code
            UpdateOrCreateRule(stylesheet, "code", (rule, info) =>
            {
                // Inline styles
                var textColor = !string.IsNullOrEmpty(info.InlineCodeTextColor) ? info.InlineCodeTextColor : info.CodeTextColor;
                var bgColor = !string.IsNullOrEmpty(info.InlineCodeBackgroundColor) ? info.InlineCodeBackgroundColor : info.CodeBackgroundColor;

                if (!string.IsNullOrEmpty(textColor)) rule.Style.SetProperty("color", textColor);
                if (!string.IsNullOrEmpty(bgColor)) rule.Style.SetProperty("background-color", bgColor);
                if (!string.IsNullOrEmpty(info.CodeFontFamily)) rule.Style.SetProperty("font-family", info.CodeFontFamily);
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, "pre code", (rule, info) =>
            {
                if (info.IsCodeBlockOverrideEnabled)
                {
                    if (!string.IsNullOrEmpty(info.BlockCodeTextColor)) rule.Style.SetProperty("color", info.BlockCodeTextColor, "important");
                    if (!string.IsNullOrEmpty(info.BlockCodeBackgroundColor)) rule.Style.SetProperty("background-color", info.BlockCodeBackgroundColor, "important");
                }
                else
                {
                    // If override is disabled, we remove these properties from the rule to let highlight.js theme win
                    // but we might want to preserve them if they were already there?
                    // For now, follow the requirement: theme takes precedence.
                    rule.Style.RemoveProperty("color");
                    rule.Style.RemoveProperty("background-color");
                }
            }, styleInfo);

            UpdateHeadingNumbering(stylesheet, styleInfo);

            // Footnotes
            // Marker
            UpdateOrCreateRule(stylesheet, ".footnote-ref", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.Footnote.MarkerTextColor)) rule.Style.SetProperty("color", info.Footnote.MarkerTextColor);
                else rule.Style.RemoveProperty("color");

                if (info.Footnote.IsMarkerBold) rule.Style.SetProperty("font-weight", "bold");
                else rule.Style.RemoveProperty("font-weight");

                // Ensure superscript appearance for both number and brackets
                rule.Style.SetProperty("vertical-align", "super");
                rule.Style.SetProperty("font-size", "smaller");
                rule.Style.SetProperty("text-decoration", "none");
            }, styleInfo);

            // Prevent double superscripting when child sup exists
            UpdateOrCreateRule(stylesheet, ".footnote-ref sup", (rule, info) =>
            {
                rule.Style.SetProperty("vertical-align", "baseline");
                rule.Style.SetProperty("font-size", "100%");
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, ".footnote-ref::before", (rule, info) =>
            {
                if (info.Footnote.HasMarkerBrackets) rule.Style.SetProperty("content", "'['");
                else rule.Style.RemoveProperty("content");
            }, styleInfo);

            UpdateOrCreateRule(stylesheet, ".footnote-ref::after", (rule, info) =>
            {
                if (info.Footnote.HasMarkerBrackets) rule.Style.SetProperty("content", "']'");
                else rule.Style.RemoveProperty("content");
            }, styleInfo);

            // Area
            UpdateOrCreateRule(stylesheet, ".footnotes", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.Footnote.AreaFontSize)) rule.Style.SetProperty("font-size", info.Footnote.AreaFontSize);
                else rule.Style.RemoveProperty("font-size");

                if (!string.IsNullOrEmpty(info.Footnote.AreaTextColor)) rule.Style.SetProperty("color", info.Footnote.AreaTextColor);
                else rule.Style.RemoveProperty("color");

                if (!string.IsNullOrEmpty(info.Footnote.AreaMarginTop)) rule.Style.SetProperty("margin-top", info.Footnote.AreaMarginTop);
                else rule.Style.RemoveProperty("margin-top");
            }, styleInfo);

            // Area Divider (HR)
            UpdateOrCreateRule(stylesheet, ".footnotes hr", (rule, info) =>
            {
                bool hasBorder = !string.IsNullOrEmpty(info.Footnote.AreaBorderTopWidth) ||
                                 !string.IsNullOrEmpty(info.Footnote.AreaBorderTopColor) ||
                                 !string.IsNullOrEmpty(info.Footnote.AreaBorderTopStyle);

                if (hasBorder)
                {
                    rule.Style.SetProperty("border", "0"); // Reset default HR style
                    var width = !string.IsNullOrEmpty(info.Footnote.AreaBorderTopWidth) ? info.Footnote.AreaBorderTopWidth : "1px";
                    var style = !string.IsNullOrEmpty(info.Footnote.AreaBorderTopStyle) ? info.Footnote.AreaBorderTopStyle : "solid";
                    var color = !string.IsNullOrEmpty(info.Footnote.AreaBorderTopColor) ? info.Footnote.AreaBorderTopColor : "currentColor";
                    rule.Style.SetProperty("border-top", $"{width} {style} {color}");
                }
                else
                {
                    rule.Style.RemoveProperty("border");
                    rule.Style.RemoveProperty("border-top");
                }
            }, styleInfo);

            // List Item
            UpdateOrCreateRule(stylesheet, ".footnotes li", (rule, info) =>
            {
                if (!string.IsNullOrEmpty(info.Footnote.ListItemLineHeight)) rule.Style.SetProperty("line-height", info.Footnote.ListItemLineHeight);
                else rule.Style.RemoveProperty("line-height");
            }, styleInfo);

            // Back Link
            UpdateOrCreateRule(stylesheet, ".footnote-back-ref", (rule, info) =>
            {
                if (!info.Footnote.IsBackLinkVisible) rule.Style.SetProperty("display", "none");
                else rule.Style.RemoveProperty("display");
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

            return generatedCss;
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


        private void ParseBodyStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "body");
            if (rule != null)
            {
                styleInfo.BodyTextColor = GetColorHexFromRule(rule, "color");
                styleInfo.BodyBackgroundColor = GetColorHexFromRule(rule, "background-color");
                styleInfo.BodyFontSize = rule.Style.GetPropertyValue("font-size");
            }
        }

        private void ParseParagraphStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "p");
            if (rule != null)
            {
                styleInfo.ParagraphLineHeight = rule.Style.GetPropertyValue("line-height");
                styleInfo.ParagraphMarginBottom = rule.Style.GetPropertyValue("margin-bottom");
                styleInfo.ParagraphTextIndent = rule.Style.GetPropertyValue("text-indent");
            }
        }

        private void ParseTitleStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "#page-title");
            if (rule != null)
            {
                styleInfo.TitleTextColor = GetColorHexFromRule(rule, "color");
                styleInfo.TitleFontSize = rule.Style.GetPropertyValue("font-size");
                styleInfo.TitleFontFamily = rule.Style.GetPropertyValue("font-family");
                styleInfo.TitleAlignment = rule.Style.GetPropertyValue("text-align");
                styleInfo.TitleMarginBottom = rule.Style.GetPropertyValue("margin-bottom");
                styleInfo.TitleStyleFlags = GetStyleFlags(rule);
            }
        }

        private void ParseHeadingStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
                if (rule != null)
                {
                    styleInfo.HeadingTextColors[selector] = GetColorHexFromRule(rule, "color");
                    styleInfo.HeadingFontSizes[selector] = rule.Style.GetPropertyValue("font-size");
                    styleInfo.HeadingFontFamilies[selector] = rule.Style.GetPropertyValue("font-family");
                    styleInfo.HeadingAlignments[selector] = rule.Style.GetPropertyValue("text-align");
                    styleInfo.HeadingStyleFlags[selector] = GetStyleFlags(rule);
                }
            }
        }

        private void ParseBlockquoteStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "blockquote");
            if (rule != null)
            {
                styleInfo.QuoteTextColor = GetColorHexFromRule(rule, "color");
                styleInfo.QuoteBackgroundColor = GetColorHexFromRule(rule, "background-color");
                styleInfo.QuoteBorderWidth = rule.Style.GetPropertyValue("border-left-width");
                styleInfo.QuoteBorderStyle = rule.Style.GetPropertyValue("border-left-style");
                styleInfo.QuoteBorderColor = GetColorHexFromRule(rule, "border-left-color");
            }
        }

        private void ParseListStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var ulRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "ul");
            if (ulRule != null)
            {
                styleInfo.ListMarkerType = ulRule.Style.GetPropertyValue("list-style-type");
                styleInfo.ListIndent = ulRule.Style.GetPropertyValue("padding-left");
            }

            var olRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "ol");
            if (olRule != null)
            {
                styleInfo.NumberedListMarkerType = olRule.Style.GetPropertyValue("list-style-type");
            }

            var markerRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "li::marker");
            if (markerRule != null)
            {
                styleInfo.ListMarkerSize = markerRule.Style.GetPropertyValue("font-size");
            }
        }

        private void ParseTableStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var thTdRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "th, td");
            if (thTdRule != null)
            {
                styleInfo.TableBorderWidth = thTdRule.Style.GetPropertyValue("border-width");
                styleInfo.TableBorderColor = GetColorHexFromRule(thTdRule, "border-color");
                styleInfo.TableBorderStyle = thTdRule.Style.GetPropertyValue("border-style");
                styleInfo.TableCellPadding = thTdRule.Style.GetPropertyValue("padding");
            }

            var thRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "th");
            if (thRule != null)
            {
                styleInfo.TableHeaderBackgroundColor = GetColorHexFromRule(thRule, "background-color");
                styleInfo.TableHeaderTextColor = GetColorHexFromRule(thRule, "color");
                styleInfo.TableHeaderFontSize = thRule.Style.GetPropertyValue("font-size");
                styleInfo.TableHeaderAlignment = thRule.Style.GetPropertyValue("text-align");
            }
        }

        private void ParseCodeStyles(ICssStyleSheet stylesheet, CssStyleInfo styleInfo)
        {
            var codeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "code");
            if (codeRule != null)
            {
                styleInfo.CodeTextColor = GetColorHexFromRule(codeRule, "color");
                styleInfo.CodeBackgroundColor = GetColorHexFromRule(codeRule, "background-color");
                styleInfo.InlineCodeTextColor = styleInfo.CodeTextColor;
                styleInfo.InlineCodeBackgroundColor = styleInfo.CodeBackgroundColor;
                styleInfo.CodeFontFamily = codeRule.Style.GetPropertyValue("font-family");
            }

            var preCodeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "pre code");
            if (preCodeRule != null)
            {
                styleInfo.BlockCodeTextColor = GetColorHexFromRule(preCodeRule, "color");
                styleInfo.BlockCodeBackgroundColor = GetColorHexFromRule(preCodeRule, "background-color");
            }
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
            var markerRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnote-ref");
            if (markerRule != null)
            {
                styleInfo.Footnote.MarkerTextColor = GetColorHexFromRule(markerRule, "color");
                var fontWeight = markerRule.Style.GetPropertyValue("font-weight");
                styleInfo.Footnote.IsMarkerBold = fontWeight == "bold" || (int.TryParse(fontWeight, out var w) && w >= 700);
            }

            var markerBeforeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnote-ref::before");
            styleInfo.Footnote.HasMarkerBrackets = markerBeforeRule?.Style.GetPropertyValue("content")?.Contains("[") == true;

            var areaRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnotes");
            if (areaRule != null)
            {
                styleInfo.Footnote.AreaFontSize = areaRule.Style.GetPropertyValue("font-size");
                styleInfo.Footnote.AreaTextColor = GetColorHexFromRule(areaRule, "color");
                styleInfo.Footnote.AreaMarginTop = areaRule.Style.GetPropertyValue("margin-top");
            }

            var hrRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnotes hr");
            if (hrRule != null)
            {
                styleInfo.Footnote.AreaBorderTopWidth = hrRule.Style.GetPropertyValue("border-top-width");
                if (string.IsNullOrEmpty(styleInfo.Footnote.AreaBorderTopWidth) && !string.IsNullOrEmpty(hrRule.Style.GetPropertyValue("border-top")))
                {
                    // Parse shorthand if specific prop is missing (simplified)
                    styleInfo.Footnote.AreaBorderTopWidth = "1px";
                }
                styleInfo.Footnote.AreaBorderTopStyle = hrRule.Style.GetPropertyValue("border-top-style");
                styleInfo.Footnote.AreaBorderTopColor = GetColorHexFromRule(hrRule, "border-top-color");
            }

            var liRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnotes li");
            styleInfo.Footnote.ListItemLineHeight = liRule?.Style.GetPropertyValue("line-height");

            var backLinkRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnote-back-ref");
            if (backLinkRule != null)
            {
                styleInfo.Footnote.IsBackLinkVisible = backLinkRule.Style.GetPropertyValue("display") != "none";
            }
            else
            {
                // デフォルトは表示
                styleInfo.Footnote.IsBackLinkVisible = true;
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
