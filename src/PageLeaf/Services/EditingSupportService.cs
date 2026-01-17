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
        private static readonly Regex TaskListRegex = new Regex(@"^(\s*)([*+-])\s+\[[ xX]\]\s+");
        // コードブロック開始: 任意の空白 + バックティックまたはチルダ3つ以上
        private static readonly Regex CodeBlockStartRegex = new Regex(@"^(\s*)(?:`{3,}|~{3,})");

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

            // タスクリスト（通常の順序なしリストより先に判定）
            var taskMatch = TaskListRegex.Match(currentLine);
            if (taskMatch.Success)
            {
                var indent = taskMatch.Groups[1].Value;
                var marker = taskMatch.Groups[2].Value;

                // 記号のみ（タスクリスト終了）
                if (currentLine.Trim() == marker + " [ ]" || currentLine.Trim() == marker + " [x]" || currentLine.Trim() == marker + " [X]")
                {
                    return string.Empty;
                }

                return indent + marker + " [ ] ";
            }

            // 順序なしリスト
            var unorderedMatch = UnorderedListRegex.Match(currentLine);
            if (unorderedMatch.Success)
            {
                var indent = unorderedMatch.Groups[1].Value;
                var marker = unorderedMatch.Groups[2].Value;
                var spaces = unorderedMatch.Groups[3].Value;

                // 内容が空（インデント+記号+空白のみ）であればリスト終了
                if (currentLine.Trim() == marker)
                {
                    return string.Empty;
                }

                return indent + marker + spaces;
            }

            // 順序付きリスト
            var orderedMatch = OrderedListRegex.Match(currentLine);
            if (orderedMatch.Success)
            {
                var indent = orderedMatch.Groups[1].Value;
                var numberStr = orderedMatch.Groups[2].Value;
                var spaces = orderedMatch.Groups[3].Value;

                // 内容が空（インデント+数字+.+空白のみ）であればリスト終了
                if (currentLine.Trim() == numberStr + ".")
                {
                    return string.Empty;
                }

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
        /// 指定された行がコードブロックの開始（バックティック3つ）であるかどうかを判定します。
        /// </summary>
        /// <param name="currentLine">判定対象の行文字列。</param>
        /// <returns>コードブロックの開始であれば true。</returns>
        public bool IsCodeBlockStart(string currentLine)
        {
            if (string.IsNullOrEmpty(currentLine)) return false;
            return CodeBlockStartRegex.IsMatch(currentLine);
        }

        /// <summary>
        /// 設定に基づいたインデント文字列を取得します。
        /// </summary>
        /// <param name="settings">アプリケーション設定。</param>
        /// <returns>インデント文字列。</returns>
        public string GetIndentString(PageLeaf.Models.ApplicationSettings settings)
        {
            if (settings.UseSpacesForIndent)
            {
                return new string(' ', settings.IndentSize);
            }
            return "\t";
        }

        /// <summary>
        /// 行頭のインデントを1レベル分削除します。
        /// </summary>
        /// <param name="line">対象の行。</param>
        /// <param name="settings">アプリケーション設定。</param>
        /// <returns>インデント削除後の行。</returns>
        public string DecreaseIndent(string line, PageLeaf.Models.ApplicationSettings settings)
        {
            if (string.IsNullOrEmpty(line)) return line;

            // タブで始まる場合
            if (line.StartsWith("\t"))
            {
                return line.Substring(1);
            }

            // スペースで始まる場合、設定されたインデント幅分削除を試みる
            int spaceCount = 0;
            while (spaceCount < settings.IndentSize && spaceCount < line.Length && line[spaceCount] == ' ')
            {
                spaceCount++;
            }

            if (spaceCount > 0)
            {
                return line.Substring(spaceCount);
            }

            return line;
        }

        /// <summary>
        /// 行頭に1レベル分のインデントを追加します。
        /// </summary>
        /// <param name="line">対象の行。</param>
        /// <param name="settings">アプリケーション設定。</param>
        /// <returns>インデント追加後の行。</returns>
        public string IncreaseIndent(string line, PageLeaf.Models.ApplicationSettings settings)
        {
            var indent = GetIndentString(settings);
            return indent + line;
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
    }
}

