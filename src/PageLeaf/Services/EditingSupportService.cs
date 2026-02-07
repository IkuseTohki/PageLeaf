using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PageLeaf.Services
{
    /// <summary>
    /// エディタの編集支援機能（オートインデント、オートリスト等）のロジックを提供するサービスです。
    /// </summary>
    public class EditingSupportService : IEditingSupportService
    {
        // 順序なしリスト: 任意の空白 + (* or - or +) + 1つ以上の空白
        private static readonly Regex UnorderedListRegex = new Regex(@"^(\s*)([*+-])(\s+)");
        // 順序付きリスト: 任意の空白 + 数字 + . + 1つ以上の空白
        private static readonly Regex OrderedListRegex = new Regex(@"^(\s*)(\d+)\.(\s+)");
        // タスクリスト: 任意の空白 + (* or - or +) + 1つ以上の空白 + [ ] または [x] + 1つ以上の空白
        // グループ3にチェック状態 ([x] の中身) をキャプチャするように修正
        private static readonly Regex TaskListRegex = new Regex(@"^(\s*)([*+-])\s+\[([ xX])\]\s+");
        // 引用: 任意の空白 + > + (任意の >) + 任意の空白
        private static readonly Regex BlockquoteRegex = new Regex(@"^(\s*)(>+)(\s*)");

        /// <summary>
        /// 指定された行のインデント（先頭の空白文字列）を取得します。
        /// </summary>
        /// <param name="currentLine">判定対象の行文字列。</param>
        /// <returns>インデント文字列。</returns>
        public string GetAutoIndent(string currentLine)
        {
            if (string.IsNullOrEmpty(currentLine)) return string.Empty;

            var sb = new StringBuilder();
            foreach (var c in currentLine)
            {
                if (c == ' ' || c == '\t')
                {
                    sb.Append(c);
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 指定された行がリスト形式であれば、次の行に挿入すべきリストマーカーを取得します。
        /// </summary>
        /// <param name="currentLine">判定対象の行文字列。</param>
        /// <returns>次行のマーカー文字列。リストでない場合は null。リスト終了時は空文字列。</returns>
        public string? GetAutoListMarker(string currentLine)
        {
            if (string.IsNullOrEmpty(currentLine)) return null;

            // 引用 (Blockquote)
            var blockquoteMatch = BlockquoteRegex.Match(currentLine);
            if (blockquoteMatch.Success)
            {
                var indent = blockquoteMatch.Groups[1].Value;
                var markers = blockquoteMatch.Groups[2].Value;

                // 内容が空（マーカーのみ）であれば終了
                if (currentLine.Trim() == markers) return string.Empty;

                return indent + markers + " ";
            }

            // タスクリスト
            var taskMatch = TaskListRegex.Match(currentLine);
            if (taskMatch.Success)
            {
                // 内容が空（インデント + マーカー + [ ] のみ）であれば終了
                if (currentLine.Length == taskMatch.Length) return string.Empty;

                var indent = taskMatch.Groups[1].Value;
                var marker = taskMatch.Groups[2].Value;
                return indent + marker + " [ ] ";
            }

            // 順序なしリスト
            var unorderedMatch = UnorderedListRegex.Match(currentLine);
            if (unorderedMatch.Success)
            {
                // 内容が空であれば終了
                if (currentLine.Length == unorderedMatch.Length) return string.Empty;

                var indent = unorderedMatch.Groups[1].Value;
                var marker = unorderedMatch.Groups[2].Value;
                var spaces = unorderedMatch.Groups[3].Value;
                return indent + marker + spaces;
            }

            // 順序付きリスト
            var orderedMatch = OrderedListRegex.Match(currentLine);
            if (orderedMatch.Success)
            {
                // 内容が空であれば終了
                if (currentLine.Length == orderedMatch.Length) return string.Empty;

                var indent = orderedMatch.Groups[1].Value;
                var numberStr = orderedMatch.Groups[2].Value;
                var spaces = orderedMatch.Groups[3].Value;

                if (int.TryParse(numberStr, out int number))
                {
                    return indent + (number + 1) + "." + spaces;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定された文字に対して、自動補完すべき対となる文字を取得します。
        /// </summary>
        /// <param name="input">入力された文字。</param>
        /// <returns>対となる文字. 存在しない場合は null。</returns>
        public char? GetPairCharacter(char input)
        {
            return input switch
            {
                '(' => ')',
                '[' => ']',
                '{' => '}',
                '"' => '"',
                '\'' => '\'',
                _ => (char?)null
            };
        }

        /// <summary>
        /// 指定された行の見出しレベルを切り替えます。
        /// </summary>
        public string ToggleHeading(string line, int level)
        {
            if (line == null) return string.Empty;
            if (level < 1 || level > 6) return line;

            var trimmed = line.Trim();
            int currentLevel = 0;
            while (currentLevel < trimmed.Length && trimmed[currentLevel] == '#')
            {
                currentLevel++;
            }

            // すでに見出しだった場合、その記号部分を取り除く
            var content = trimmed.Substring(currentLevel).Trim();

            // 同じレベルなら解除
            if (currentLevel == level)
            {
                return content;
            }

            // 新しいレベルの見出しを付与
            return new string('#', level) + " " + content;
        }

        /// <summary>
        /// TSV または CSV 形式のテキストを Markdown テーブル形式に変換します。
        /// </summary>
        public string? ConvertToMarkdownTable(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            // 改行で分割
            var rawLines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (rawLines.Length == 0) return null;

            // セパレータの決定 (タブ優先)
            char separator = text.Contains('\t') ? '\t' : ',';

            // 少なくとも1つのセパレータが含まれているか確認
            if (!rawLines[0].Contains(separator)) return null;

            // 各行をセルに分割し、最大列数を特定
            var parsedLines = rawLines.Select(l => l.Split(separator).Select(c => c.Trim()).ToList()).ToList();
            int maxCols = parsedLines.Max(l => l.Count);

            var result = new StringBuilder();
            for (int i = 0; i < parsedLines.Count; i++)
            {
                var cells = parsedLines[i];

                // 最大列数に足りない場合は空セルを追加
                while (cells.Count < maxCols)
                {
                    cells.Add(string.Empty);
                }

                // 行の構築
                result.Append("| ");
                result.Append(string.Join(" | ", cells));
                result.Append(" |");

                if (i < parsedLines.Count - 1)
                {
                    result.AppendLine();
                }

                // ヘッダー行の直後に区切り行を挿入
                if (i == 0)
                {
                    result.Append("| ");
                    result.Append(string.Join(" | ", Enumerable.Repeat("---", maxCols)));
                    result.Append(" |");
                    result.AppendLine();
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 改ページ用のHTMLタグ文字列を取得します。
        /// </summary>
        public string GetPageBreakString()
        {
            return "<div style=\"page-break-after: always;\"></div>";
        }

        public bool ShouldAutoContinueList(string line, int cursorOffset)
        {
            // まず、リストとして有効か（マーカーが生成されるか、またはリスト終了動作になるか）確認
            if (GetAutoListMarker(line) == null) return false;

            // カーソル位置が本文開始位置（マーカーの後ろ）にあるか判定

            // 1. Task List
            var taskMatch = TaskListRegex.Match(line);
            if (taskMatch.Success)
            {
                return cursorOffset >= taskMatch.Length;
            }

            // 2. Unordered List
            var unorderedMatch = UnorderedListRegex.Match(line);
            if (unorderedMatch.Success)
            {
                return cursorOffset >= unorderedMatch.Length;
            }

            // 3. Ordered List
            var orderedMatch = OrderedListRegex.Match(line);
            if (orderedMatch.Success)
            {
                return cursorOffset >= orderedMatch.Length;
            }

            // 4. Blockquote
            var blockquoteMatch = BlockquoteRegex.Match(line);
            if (blockquoteMatch.Success)
            {
                return cursorOffset >= blockquoteMatch.Length;
            }

            // マッチしない場合（通常あり得ないが、念のため）
            return false;
        }

        public bool ShouldAutoIndent(string line, int cursorOffset)
        {
            // 行の絶対的な先頭 (offset 0) で Enter を押した場合は、
            // オートインデント（空白の引き継ぎ）を行わず、純粋な空行を挿入したいので false を返す。
            return cursorOffset != 0;
        }

        public string ToggleTaskList(string line)
        {
            if (line == null) return string.Empty;

            // 1. Check if it is a task list
            var taskMatch = TaskListRegex.Match(line);
            if (taskMatch.Success)
            {
                var indent = taskMatch.Groups[1].Value;
                var marker = taskMatch.Groups[2].Value;
                var checkState = taskMatch.Groups[3].Value; // space or x or X
                var content = line.Substring(taskMatch.Length);

                // If content is empty (just marker), remove the list marker (VSCode style)
                if (string.IsNullOrEmpty(content))
                {
                    return indent + marker + " ";
                }

                // Toggle state
                bool isChecked = checkState.Trim().Length > 0; // if not space, it is checked
                char newCheckState = isChecked ? ' ' : 'x';

                return $"{indent}{marker} [{newCheckState}] {content}";
            }

            // 2. Check if it is an unordered list
            var unorderedMatch = UnorderedListRegex.Match(line);
            if (unorderedMatch.Success)
            {
                var indent = unorderedMatch.Groups[1].Value;
                var marker = unorderedMatch.Groups[2].Value;
                var content = line.Substring(unorderedMatch.Length);

                return $"{indent}{marker} [ ] {content}";
            }

            // 3. Normal text -> Task List
            var currentIndent = GetAutoIndent(line);
            var text = line.TrimStart();
            return $"{currentIndent}- [ ] {text}";
        }

        public string GetShiftEnterInsertion()
        {
            // 改ページ用タグ + 改行
            // 視認性を考慮し、タグの後に改行を入れて挿入する
            return GetPageBreakString() + "\r\n";
        }

        public string FormatTableLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;
            if (!line.Contains('|')) return line;

            var parts = Regex.Split(line, @"(?<!\\)\|");
            if (parts.Length < 2) return line;

            var sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (i > 0)
                {
                    sb.Append('|');
                }

                // First part: Keep as is (indent)
                if (i == 0)
                {
                    sb.Append(part);
                    continue;
                }

                // Last part: Keep as is, unless it's just whitespace and not meaningful
                if (i == parts.Length - 1)
                {
                    sb.Append(part);
                    continue;
                }

                // Middle parts: Cell content. Pad with spaces.
                sb.Append(" " + part.Trim() + " ");
            }
            return sb.ToString();
        }

        public int GetNextCellOffset(string line, int currentOffset)
        {
            if (string.IsNullOrEmpty(line)) return 0;
            var matches = Regex.Matches(line, @"(?<!\\)\|");
            foreach (Match match in matches)
            {
                if (match.Index >= currentOffset)
                {
                    int pos = match.Index + 1;
                    if (pos < line.Length && line[pos] == ' ') pos++;
                    // もし現在の位置と同じなら（例：| の直後にいる場合）、さらに次のパイプを探す
                    if (pos <= currentOffset) continue;
                    return pos;
                }
            }
            return line.Length;
        }

        public int GetPreviousCellOffset(string line, int currentOffset)
        {
            if (string.IsNullOrEmpty(line)) return 0;
            var matches = Regex.Matches(line, @"(?<!\\)\|").Cast<Match>().ToList();
            var before = matches.Where(m => m.Index < currentOffset).ToList();

            if (before.Count >= 2)
            {
                // 現在のセルの開始パイプの、さらに一つ前のパイプを見つける
                var target = before[before.Count - 2];
                int pos = target.Index + 1;
                if (pos < line.Length && line[pos] == ' ') pos++;
                return pos;
            }

            return 0;
        }

        public string EnforceEmptyLineAtEnd(string text)
        {
            if (text == null) return Environment.NewLine;
            if (text.EndsWith(Environment.NewLine)) return text;
            if (text.EndsWith("\n") || text.EndsWith("\r")) return text + (text.EndsWith("\r") ? "\n" : "");

            return text + Environment.NewLine;
        }

        public bool ShouldSkipClosingCharacter(char input, string fullText, int caretIndex)
        {
            if (string.IsNullOrEmpty(fullText) || caretIndex >= fullText.Length) return false;

            // スキップ対象の閉じ記号
            char[] closingChars = { ']', ')', '}', '"', '\'', '`' };
            if (Array.IndexOf(closingChars, input) == -1) return false;

            // カーソル直後の文字と比較
            return fullText[caretIndex] == input;
        }
    }
}

