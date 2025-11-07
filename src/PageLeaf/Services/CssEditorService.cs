using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using PageLeaf.Models;
using System.Linq;

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
                styleInfo.BodyTextColor = bodyRule.Style.GetPropertyValue("color");
                styleInfo.BodyBackgroundColor = bodyRule.Style.GetPropertyValue("background-color");
            }

            return styleInfo;
        }
    }
}
