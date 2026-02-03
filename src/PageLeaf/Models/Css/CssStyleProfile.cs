using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using System.Collections.Generic;
using System.Linq;

using ListStyle = PageLeaf.Models.Css.Elements.ListStyle;

namespace PageLeaf.Models.Css
{
    /// <summary>
    /// CSSドメインの集約ルートです。
    /// 全ての要素スタイルを統合的に管理し、スタイルシートとの相互変換を担います。
    /// </summary>
    public class CssStyleProfile
    {
        public BodyStyle Body { get; } = new BodyStyle();
        public TitleStyle Title { get; } = new TitleStyle();
        public ParagraphStyle Paragraph { get; } = new ParagraphStyle();
        public Dictionary<string, HeadingStyle> Headings { get; } = new Dictionary<string, HeadingStyle>
        {
            { "h1", new HeadingStyle() },
            { "h2", new HeadingStyle() },
            { "h3", new HeadingStyle() },
            { "h4", new HeadingStyle() },
            { "h5", new HeadingStyle() },
            { "h6", new HeadingStyle() }
        };
        public ListStyle List { get; } = new ListStyle();
        public BlockquoteStyle Blockquote { get; } = new BlockquoteStyle();
        public TableStyle Table { get; } = new TableStyle();
        public CodeStyle Code { get; } = new CodeStyle();
        public FootnoteStyle Footnote { get; } = new FootnoteStyle();

        // プレビュー制御用のフラグ（ドメイン知識）
        public bool IsCodeBlockOverrideEnabled { get; set; }
        public Dictionary<string, bool> HeadingNumberingStates { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// 指定されたスタイルシートから全ての要素スタイルを読み込みます。
        /// </summary>
        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // 各要素モデルへの委譲
            var bodyRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "body");
            if (bodyRule != null) Body.UpdateFrom(bodyRule);

            var titleRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "#page-title");
            if (titleRule != null) Title.UpdateFrom(titleRule);

            var pRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "p");
            if (pRule != null) Paragraph.UpdateFrom(pRule);

            foreach (var kvp in Headings)
            {
                var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == kvp.Key);
                if (rule != null) kvp.Value.UpdateFrom(rule);
            }

            var quoteRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "blockquote");
            if (quoteRule != null) Blockquote.UpdateFrom(quoteRule);

            // 構造が複雑な要素はシート全体を渡す
            List.UpdateFrom(stylesheet);
            Table.UpdateFrom(stylesheet);
            Code.UpdateFrom(stylesheet);
            Footnote.UpdateFrom(stylesheet);

            // 見出し採番状態の解析（既存ロジックの移植）
            ParseNumberingStates(stylesheet);
        }

        /// <summary>
        /// 全てのプロパティを指定されたスタイルシートへ反映します。
        /// </summary>
        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // 各要素モデルへの委譲
            Body.ApplyTo(GetOrCreateRule(stylesheet, "body"));
            Title.ApplyTo(GetOrCreateRule(stylesheet, "#page-title"));
            Paragraph.ApplyTo(GetOrCreateRule(stylesheet, "p"));

            foreach (var heading in Headings.Values)
            {
                // Headings のキー（h1-h6）を取得して適用
                var selector = Headings.First(x => x.Value == heading).Key;
                heading.ApplyTo(GetOrCreateRule(stylesheet, selector));
            }

            Blockquote.ApplyTo(GetOrCreateRule(stylesheet, "blockquote"));

            List.ApplyTo(stylesheet);
            Table.ApplyTo(stylesheet);

            // CodeBlockOverride の同期
            Code.IsBlockOverrideEnabled = IsCodeBlockOverrideEnabled;
            Code.ApplyTo(stylesheet);

            Footnote.ApplyTo(stylesheet);
        }

        private ICssStyleRule GetOrCreateRule(ICssStyleSheet stylesheet, string selector)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
            if (rule == null)
            {
                stylesheet.Insert($"{selector} {{}}", stylesheet.Rules.Length);
                rule = stylesheet.Rules.OfType<ICssStyleRule>().Last();
            }
            return rule;
        }

        private void ParseNumberingStates(ICssStyleSheet stylesheet)
        {
            for (int i = 1; i <= 6; i++)
            {
                var selector = $"h{i}";
                var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
                var beforeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == $"{selector}::before");

                HeadingNumberingStates[selector] =
                    rule?.Style.GetPropertyValue("counter-increment")?.Contains(selector) == true &&
                    beforeRule?.Style.GetPropertyValue("content")?.Contains($"counter({selector})") == true;
            }
        }
    }
}
