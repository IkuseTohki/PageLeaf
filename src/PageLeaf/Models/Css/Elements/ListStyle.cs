using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System.Linq;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// リスト要素（ul, ol, li）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class ListStyle
    {
        public string? UnorderedListMarkerType { get; set; }
        public string? OrderedListMarkerType { get; set; }
        public CssSize? MarkerSize { get; set; }
        public CssSize? ListIndent { get; set; }

        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            var ulRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "ul");
            if (ulRule != null) UnorderedListMarkerType = ulRule.Style.GetPropertyValue("list-style-type");

            var olRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "ol");
            if (olRule != null)
            {
                var type = olRule.Style.GetPropertyValue("list-style-type");
                var reset = olRule.Style.GetPropertyValue("counter-reset");
                if (type == "none" && reset != null && reset.Contains("item"))
                    OrderedListMarkerType = "decimal-nested";
                else
                    OrderedListMarkerType = type;
            }

            var liRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "li");
            if (liRule != null)
            {
                var indent = liRule.Style.GetPropertyValue("margin-left");
                if (!string.IsNullOrEmpty(indent)) ListIndent = CssSize.Parse(indent);

                var size = liRule.Style.GetPropertyValue("font-size");
                if (!string.IsNullOrEmpty(size)) MarkerSize = CssSize.Parse(size);
            }

            // Fallback for ListIndent from ul/ol padding-left (Legacy compatibility)
            if (ListIndent == null)
            {
                var indentValue = ulRule?.Style.GetPropertyValue("padding-left") ?? olRule?.Style.GetPropertyValue("padding-left");
                if (!string.IsNullOrEmpty(indentValue))
                {
                    ListIndent = CssSize.Parse(indentValue);
                }
            }
        }

        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // ul
            if (!string.IsNullOrEmpty(UnorderedListMarkerType))
            {
                var rule = GetOrCreateRule(stylesheet, "ul");
                rule.Style.SetProperty("list-style-type", UnorderedListMarkerType);
            }

            // ol
            if (!string.IsNullOrEmpty(OrderedListMarkerType))
            {
                var rule = GetOrCreateRule(stylesheet, "ol");
                if (OrderedListMarkerType == "decimal-nested")
                {
                    rule.Style.SetProperty("list-style-type", "none");
                    rule.Style.SetProperty("counter-reset", "item");
                }
                else
                {
                    rule.Style.RemoveProperty("counter-reset");
                    rule.Style.SetProperty("list-style-type", OrderedListMarkerType);
                }

                // Nested styles (li::before etc.) are handled by CssEditorService logic for now 
                // but should be moved here later for full encapsulation.
            }

            // li
            if (MarkerSize != null)
            {
                var rule = GetOrCreateRule(stylesheet, "li");
                rule.Style.SetProperty("font-size", MarkerSize.ToString());
            }

            // List Indent (applied to ul/ol padding-left for consistency)
            if (ListIndent != null)
            {
                var ulRule = GetOrCreateRule(stylesheet, "ul");
                ulRule.Style.SetProperty("padding-left", ListIndent.ToString());

                var olRule = GetOrCreateRule(stylesheet, "ol");
                olRule.Style.SetProperty("padding-left", ListIndent.ToString());
            }
        }

        private ICssStyleRule GetOrCreateRule(ICssStyleSheet stylesheet, string selector)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
            if (rule == null)
            {
                stylesheet.Insert(selector + " {}", stylesheet.Rules.Length);
                rule = stylesheet.Rules.OfType<ICssStyleRule>().Last();
            }
            return rule;
        }
    }
}
