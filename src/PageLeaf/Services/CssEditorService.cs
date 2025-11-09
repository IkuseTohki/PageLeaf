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
