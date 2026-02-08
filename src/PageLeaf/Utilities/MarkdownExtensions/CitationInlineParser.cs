using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace PageLeaf.Utilities.MarkdownExtensions
{
    /// <summary>
    /// [cite: ...] 形式の引用元を解析するパーサーです。
    /// </summary>
    public class CitationInlineParser : InlineParser
    {
        public CitationInlineParser()
        {
            OpeningCharacters = new[] { '[' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var startPosition = slice.Start;

            // [cite: をチェック
            if (!slice.Match("[cite:")) return false;

            var matchStart = slice.Start;
            slice.Start += "[cite:".Length;

            // 閉じカッコを探す
            int endPosition = -1;
            var currentSlice = slice;
            while (currentSlice.CurrentChar != '\0')
            {
                if (currentSlice.CurrentChar == ']')
                {
                    endPosition = currentSlice.Start;
                    break;
                }
                currentSlice.NextChar();
            }

            if (endPosition == -1)
            {
                // 閉じカッコがない場合は通常通り処理
                slice.Start = startPosition;
                return false;
            }

            // 内容を取得
            var content = new StringSlice(slice.Text, slice.Start, endPosition - 1);

            var citation = new CitationInline
            {
                Span = { Start = processor.GetSourcePosition(matchStart) },
            };

            // 内容を LiteralInline として追加
            citation.AppendChild(new LiteralInline
            {
                Content = content,
                Span = { Start = processor.GetSourcePosition(slice.Start), End = processor.GetSourcePosition(endPosition - 1) }
            });

            citation.Span.End = processor.GetSourcePosition(endPosition);
            slice.Start = endPosition + 1;

            processor.Inline = citation;
            return true;
        }
    }
}
