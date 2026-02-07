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
        public bool? HasOrderedListPeriod { get; set; }
        public CssSize? MarkerSize { get; set; }
        public CssSize? ListIndent { get; set; }
        public string? LineHeight { get; set; }

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

            var markerRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText != null && r.SelectorText.Replace(" ", "").EndsWith("li::before"));
            if (markerRule != null)
            {
                var content = markerRule.Style.GetPropertyValue("content");
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("\". \"") || content.Contains("'. '"))
                        HasOrderedListPeriod = true;
                    else if (content.Contains("\" \"") || content.Contains("' '"))
                        HasOrderedListPeriod = false;
                }
            }

            var liRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "li");
            if (liRule != null)
            {
                var indent = liRule.Style.GetPropertyValue("margin-left");
                if (!string.IsNullOrEmpty(indent)) ListIndent = CssSize.Parse(indent);

                var size = liRule.Style.GetPropertyValue("font-size");
                if (!string.IsNullOrEmpty(size)) MarkerSize = CssSize.Parse(size);

                LineHeight = liRule.Style.GetPropertyValue("line-height");
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
            var ulRule = GetOrCreateRule(stylesheet, "ul");
            if (!string.IsNullOrEmpty(UnorderedListMarkerType))
            {
                ulRule.Style.SetProperty("list-style-type", UnorderedListMarkerType);
            }
            else
            {
                ulRule.Style.RemoveProperty("list-style-type");
            }

            // ol
            var olRule = GetOrCreateRule(stylesheet, "ol");
            if (!string.IsNullOrEmpty(OrderedListMarkerType))
            {
                if (OrderedListMarkerType == "decimal-nested")
                {
                    olRule.Style.SetProperty("list-style-type", "none");
                    olRule.Style.SetProperty("counter-reset", "item");
                }
                else
                {
                    olRule.Style.RemoveProperty("counter-reset");
                    olRule.Style.SetProperty("list-style-type", OrderedListMarkerType);
                }
            }
            else
            {
                olRule.Style.RemoveProperty("list-style-type");
                olRule.Style.RemoveProperty("counter-reset");
            }

            // li
            var liRule = GetOrCreateRule(stylesheet, "li");
            if (MarkerSize != null)
            {
                liRule.Style.SetProperty("font-size", MarkerSize.ToString());
            }
            else
            {
                liRule.Style.RemoveProperty("font-size");
            }

            if (!string.IsNullOrEmpty(LineHeight))
            {
                liRule.Style.SetProperty("line-height", LineHeight);
            }
            else
            {
                liRule.Style.RemoveProperty("line-height");
            }

            // List Indent (applied to ul/ol padding-left for consistency)
            if (ListIndent != null)
            {
                ulRule.Style.SetProperty("padding-left", ListIndent.ToString());
                olRule.Style.SetProperty("padding-left", ListIndent.ToString());
            }
            else
            {
                ulRule.Style.RemoveProperty("padding-left");
                olRule.Style.RemoveProperty("padding-left");
            }
        }

        private ICssStyleRule GetOrCreateRule(ICssStyleSheet stylesheet, string selector)
        {
            var normalizedSelector = selector.Replace(" ", "");
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText != null && r.SelectorText.Replace(" ", "") == normalizedSelector);
            if (rule == null)
            {
                stylesheet.Insert(selector + " {}", stylesheet.Rules.Length);
                rule = stylesheet.Rules.OfType<ICssStyleRule>().Last();
            }
            return rule;
        }
    }
}
