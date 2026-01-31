using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// Markdownの脚注に関するユーティリティ機能を提供します。
    /// </summary>
    public static class MarkdownFootnoteHelper
    {
        private static readonly Regex FencedCodeBlockRegex = new Regex(@"(^ {0,3}(`{3,}|~{3,})[\s\S]*?\n {0,3}\2)", RegexOptions.Multiline);
        private static readonly Regex InlineCodeRegex = new Regex(@"`[^`\n]+`|``[^`\n]+``");
        private static readonly Regex FootnoteMarkerRegex = new Regex(@"\[\^([^\]]+)\](?!\:)");
        private static readonly Regex FootnoteDefinitionRegex = new Regex(@"^\[\^([^\]]+)\]:\s*(.*)$", RegexOptions.Multiline);

        /// <summary>
        /// Markdownテキスト内の脚注番号を登場順に振り直します。
        /// </summary>
        /// <param name="text">対象のMarkdownテキスト。</param>
        /// <returns>リナンバリングされたMarkdownテキスト。</returns>
        public static string Renumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // 1. コード部分（フェンスドブロック、インライン）を一時退避
            var (maskedText, placeholders) = MaskCode(text);

            // 2. 既存の定義を収集し、本文から除去
            var definitions = new Dictionary<string, string>();
            maskedText = FootnoteDefinitionRegex.Replace(maskedText, m =>
            {
                var id = m.Groups[1].Value;
                var content = m.Groups[2].Value;
                definitions[id] = content;
                return string.Empty;
            });

            // 3. 本文中のマーカーを出現順にスキャンし、新しい番号を割り当て
            var idMapping = new Dictionary<string, int>();
            var counter = 1;

            maskedText = FootnoteMarkerRegex.Replace(maskedText, m =>
            {
                var oldId = m.Groups[1].Value;
                if (!idMapping.TryGetValue(oldId, out var newId))
                {
                    newId = counter++;
                    idMapping[oldId] = newId;
                }
                return $"[^{newId}]";
            });

            // 4. 本文末尾に整理された定義リストを追加
            var resultBuilder = new StringBuilder(maskedText.TrimEnd());
            if (idMapping.Any())
            {
                resultBuilder.AppendLine();
                resultBuilder.AppendLine();
                foreach (var entry in idMapping.OrderBy(x => x.Value))
                {
                    if (definitions.TryGetValue(entry.Key, out var content))
                    {
                        resultBuilder.AppendLine($"[^{entry.Value}]: {content.Trim()}");
                    }
                    else
                    {
                        // 定義が見つからない場合は空の定義を作成
                        resultBuilder.AppendLine($"[^{entry.Value}]:");
                    }
                }
            }

            // 5. コード部分を復元
            return UnmaskCode(resultBuilder.ToString(), placeholders);
        }

        /// <summary>
        /// ドキュメント内で使用されている脚注番号の最大値 + 1 を取得します。
        /// </summary>
        /// <param name="text">対象のMarkdownテキスト。</param>
        /// <returns>次回来用の脚注番号。</returns>
        public static int GetNextFootnoteNumber(string text)
        {
            if (string.IsNullOrEmpty(text)) return 1;

            // コードブロック内を無視して最大値を探す
            var (maskedText, _) = MaskCode(text);
            var matches = FootnoteMarkerRegex.Matches(maskedText);
            var max = 0;
            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out var n))
                {
                    max = Math.Max(max, n);
                }
            }
            return max + 1;
        }

        private static (string maskedText, List<string> placeholders) MaskCode(string text)
        {
            var placeholders = new List<string>();

            // Fenced Code Blocks
            var masked = FencedCodeBlockRegex.Replace(text, m =>
            {
                var placeholder = $"__BLOCK_{placeholders.Count}__";
                placeholders.Add(m.Value);
                return placeholder;
            });

            // Inline Code
            masked = InlineCodeRegex.Replace(masked, m =>
            {
                var placeholder = $"__INLINE_{placeholders.Count}__";
                placeholders.Add(m.Value);
                return placeholder;
            });

            return (masked, placeholders);
        }

        private static string UnmaskCode(string maskedText, List<string> placeholders)
        {
            var result = maskedText;
            for (int i = 0; i < placeholders.Count; i++)
            {
                // MaskCode で追加した順に置換
                result = result.Replace($"__BLOCK_{i}__", placeholders[i]);
                result = result.Replace($"__INLINE_{i}__", placeholders[i]);
            }
            return result;
        }
    }
}
