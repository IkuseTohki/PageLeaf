using PageLeaf.Models;
using System.Collections.ObjectModel;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// チートシート画面の ViewModel。
    /// Markdown記法とショートカットキーのリストを提供します。
    /// </summary>
    public class CheatSheetViewModel : ViewModelBase
    {
        /// <summary>
        /// Markdown記法のリスト
        /// </summary>
        public ObservableCollection<CheatSheetItem> MarkdownItems { get; }

        /// <summary>
        /// ショートカットキーのリスト
        /// </summary>
        public ObservableCollection<CheatSheetItem> ShortcutItems { get; }

        public CheatSheetViewModel()
        {
            MarkdownItems = new ObservableCollection<CheatSheetItem>(GetMarkdownItems());
            ShortcutItems = new ObservableCollection<CheatSheetItem>(GetShortcutItems());
        }

        private static System.Collections.Generic.IEnumerable<CheatSheetItem> GetMarkdownItems()
        {
            return new[]
            {
                new CheatSheetItem { Category = "見出し", Syntax = "# H1", Description = "見出し 1 (文書タイトル)", RelatedShortcut = "Ctrl + 1" },
                new CheatSheetItem { Category = "見出し", Syntax = "## H2", Description = "見出し 2 (セクション)", RelatedShortcut = "Ctrl + 2" },
                new CheatSheetItem { Category = "見出し", Syntax = "### H3", Description = "見出し 3 (サブセクション)", RelatedShortcut = "Ctrl + 3" },
                new CheatSheetItem { Category = "強調", Syntax = "**太字**", Description = "太字 (Bold)", RelatedShortcut = "Ctrl + B" },
                new CheatSheetItem { Category = "強調", Syntax = "*斜体*", Description = "斜体 (Italic)", RelatedShortcut = "Ctrl + I" },
                new CheatSheetItem { Category = "強調", Syntax = "~~取り消し~~", Description = "取り消し線" },
                new CheatSheetItem { Category = "リスト", Syntax = "- リスト", Description = "箇条書きリスト" },
                new CheatSheetItem { Category = "リスト", Syntax = "1. リスト", Description = "番号付きリスト" },
                new CheatSheetItem { Category = "リスト", Syntax = "- [ ] タスク", Description = "タスクリスト (未完了)" },
                new CheatSheetItem { Category = "リスト", Syntax = "- [x] タスク", Description = "タスクリスト (完了)" },
                new CheatSheetItem { Category = "リンク・画像", Syntax = "[リンク](URL)", Description = "リンク", RelatedShortcut = "Ctrl + K" },
                new CheatSheetItem { Category = "リンク・画像", Syntax = "![Alt](ImageURL)", Description = "画像埋め込み", RelatedShortcut = "Ctrl + Shift + V" },
                new CheatSheetItem { Category = "引用・コード", Syntax = "> 引用", Description = "引用ブロック" },
                new CheatSheetItem { Category = "引用・コード", Syntax = "`code`", Description = "インラインコード" },
                new CheatSheetItem { Category = "引用・コード", Syntax = "```\ncode\n```", Description = "コードブロック" },
                new CheatSheetItem { Category = "テーブル", Syntax = "| A | B |\n|---|---|", Description = "テーブル作成" },
                new CheatSheetItem { Category = "その他", Syntax = "---", Description = "水平線" },
                new CheatSheetItem { Category = "独自機能", Syntax = "Shift + Enter", Description = "強制改ページ挿入", Note = "<div style=\"page-break-after: always;\"></div>", RelatedShortcut = "Shift + Enter" }
            };
        }

        private static System.Collections.Generic.IEnumerable<CheatSheetItem> GetShortcutItems()
        {
            return new[]
            {
                new CheatSheetItem { Category = "ファイル操作", Syntax = "Ctrl + N", Description = "新規作成" },
                new CheatSheetItem { Category = "ファイル操作", Syntax = "Ctrl + O", Description = "ファイルを開く" },
                new CheatSheetItem { Category = "ファイル操作", Syntax = "Ctrl + S", Description = "上書き保存" },
                new CheatSheetItem { Category = "ファイル操作", Syntax = "Ctrl + Shift + S", Description = "名前を付けて保存" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Ctrl + Z", Description = "元に戻す" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Ctrl + Y", Description = "やり直し" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Ctrl + B", Description = "太字" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Ctrl + I", Description = "斜体" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Ctrl + K", Description = "リンク挿入" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Ctrl + Shift + V", Description = "画像を貼り付け" },
                new CheatSheetItem { Category = "編集操作", Syntax = "Shift + Enter", Description = "強制改ページ挿入" },
                new CheatSheetItem { Category = "見出し操作", Syntax = "Ctrl + 1~6", Description = "見出し 1〜6 に設定" },
                new CheatSheetItem { Category = "表示操作", Syntax = "Alt + Shift + ←/→", Description = "編集/プレビューモード切替" },
                new CheatSheetItem { Category = "表示操作", Syntax = "Ctrl + +", Description = "ズームイン (プレビュー)" },
                new CheatSheetItem { Category = "表示操作", Syntax = "Ctrl + -", Description = "ズームアウト (プレビュー)" },
                new CheatSheetItem { Category = "表示操作", Syntax = "Ctrl + 0", Description = "ズームリセット (プレビュー)" }
            };
        }
    }
}
