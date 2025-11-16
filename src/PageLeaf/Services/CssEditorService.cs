using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using PageLeaf.Models;
using System.Linq;
using System.Text;
using System.IO;
using PageLeaf.Utilities;
using System.Windows.Media; // ColorConverter を使用するために追加
using AngleSharp.Css.Values; // AngleSharp.Css.Values.Color を使用するために追加

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
                // AngleSharpのColorオブジェクトから直接HEX形式を取得する
                var colorProperty = bodyRule.Style.GetProperty("color");
                if (colorProperty != null && colorProperty.RawValue is AngleSharp.Css.Values.Color angleSharpColor)
                {
                    styleInfo.BodyTextColor = $"#{angleSharpColor.R:X2}{angleSharpColor.G:X2}{angleSharpColor.B:X2}"; // ここを修正
                }
                else
                {
                    styleInfo.BodyTextColor = ConvertToHex(bodyRule.Style.GetPropertyValue("color")); // フォールバック
                }

                var backgroundColorProperty = bodyRule.Style.GetProperty("background-color");
                if (backgroundColorProperty != null && backgroundColorProperty.RawValue is AngleSharp.Css.Values.Color angleSharpBackgroundColor)
                {
                    styleInfo.BodyBackgroundColor = $"#{angleSharpBackgroundColor.R:X2}{angleSharpBackgroundColor.G:X2}{angleSharpBackgroundColor.B:X2}"; // ここを修正
                }
                else
                {
                    styleInfo.BodyBackgroundColor = ConvertToHex(bodyRule.Style.GetPropertyValue("background-color")); // フォールバック
                }

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
                // color
                var colorProperty = blockquoteRule.Style.GetProperty("color");
                if (colorProperty != null && colorProperty.RawValue is AngleSharp.Css.Values.Color angleSharpColor)
                {
                    styleInfo.QuoteTextColor = $"#{angleSharpColor.R:X2}{angleSharpColor.G:X2}{angleSharpColor.B:X2}";
                }
                else
                {
                    styleInfo.QuoteTextColor = ConvertToHex(blockquoteRule.Style.GetPropertyValue("color"));
                }

                // background-color
                var backgroundColorProperty = blockquoteRule.Style.GetProperty("background-color");
                if (backgroundColorProperty != null && backgroundColorProperty.RawValue is AngleSharp.Css.Values.Color angleSharpBackgroundColor)
                {
                    styleInfo.QuoteBackgroundColor = $"#{angleSharpBackgroundColor.R:X2}{angleSharpBackgroundColor.G:X2}{angleSharpBackgroundColor.B:X2}";
                }
                else
                {
                    styleInfo.QuoteBackgroundColor = ConvertToHex(blockquoteRule.Style.GetPropertyValue("background-color"));
                }

                // border-left
                styleInfo.QuoteBorderWidth = blockquoteRule.Style.GetPropertyValue("border-left-width");
                styleInfo.QuoteBorderStyle = blockquoteRule.Style.GetPropertyValue("border-left-style");

                var borderLeftColorProperty = blockquoteRule.Style.GetProperty("border-left-color");
                if (borderLeftColorProperty != null && borderLeftColorProperty.RawValue is AngleSharp.Css.Values.Color angleSharpBorderColor)
                {
                    styleInfo.QuoteBorderColor = $"#{angleSharpBorderColor.R:X2}{angleSharpBorderColor.G:X2}{angleSharpBorderColor.B:X2}";
                }
                else
                {
                    styleInfo.QuoteBorderColor = ConvertToHex(blockquoteRule.Style.GetPropertyValue("border-left-color"));
                }
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

            // 更新されたスタイルシートを文字列として出力
            using (var writer = new StringWriter())
            {
                stylesheet.ToCss(writer, new PrettyStyleFormatter()); // PrettyStyleFormatter を使用
                return writer.ToString();
            }
        }

        /// <summary>
        /// RGBAまたはRGB形式の色文字列をHEX形式に変換します。
        /// </summary>
        /// <param name="colorValue">RGBAまたはRGB形式の色文字列。</param>
        /// <returns>HEX形式の色文字列（例: #RRGGBB）。変換できない場合はnull。</returns>
        private string? ConvertToHex(string? colorValue)
        {
            if (string.IsNullOrEmpty(colorValue) || colorValue == "transparent")
            {
                return null;
            }

            // すでにHEX形式の場合はそのまま返す
            if (colorValue.StartsWith("#") && (colorValue.Length == 7 || colorValue.Length == 4))
            {
                return colorValue;
            }

            System.Windows.Media.Color color; // ここを修正
            try
            {
                // System.Windows.Media.ColorConverter.ConvertFromString は "rgb(r, g, b)" や "rgba(r, g, b, a)" 形式をパースできる
                color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(colorValue);
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch
            {
                // 変換できない場合はnullを返す
                return null;
            }
        }
    }
}
