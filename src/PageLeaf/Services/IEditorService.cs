using PageLeaf.Models;
using System;
using System.ComponentModel;

namespace PageLeaf.Services
{
    /// <summary>
    /// Markdownエディタの状態管理と操作を提供するサービスインターフェースです。
    /// テキストの編集、表示モードの切り替え、CSSの適用などを管理します。
    /// </summary>
    public interface IEditorService : INotifyPropertyChanged
    {
        /// <summary>
        /// 現在編集中のMarkdownドキュメントを取得します。
        /// </summary>
        MarkdownDocument CurrentDocument { get; }

        /// <summary>
        /// エディタに表示されている生テキストを取得または設定します。
        /// </summary>
        string EditorText { get; set; }

        /// <summary>
        /// プレビュー用に生成された一時的なHTMLファイルのパスを取得します。
        /// </summary>
        string HtmlFilePath { get; }

        /// <summary>
        /// 現在の表示モード（エディタのみ、プレビューのみ、両方など）を取得または設定します。
        /// </summary>
        DisplayMode SelectedMode { get; set; }

        /// <summary>
        /// エディタのフォントサイズを取得または設定します。
        /// </summary>
        double EditorFontSize { get; set; }

        /// <summary>
        /// Markdownエディタ（TextBox）が表示されるべきかどうかを取得します。
        /// </summary>
        bool IsMarkdownEditorVisible { get; }

        /// <summary>
        /// プレビュー（WebView2）が表示されるべきかどうかを取得します。
        /// </summary>
        bool IsViewerVisible { get; }

        /// <summary>
        /// 現在のドキュメントに変更があり、保存が必要かどうかを取得します。
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 指定されたドキュメントをエディタに読み込みます。
        /// </summary>
        /// <param name="document">読み込むMarkdownドキュメント。</param>
        void LoadDocument(MarkdownDocument document);

        /// <summary>
        /// プレビューに指定されたCSSスタイルを適用します。
        /// </summary>
        /// <param name="cssFileName">適用するCSSファイル名。</param>
        void ApplyCss(string cssFileName);

        /// <summary>
        /// エディタを初期状態（新規ドキュメント）にリセットします。
        /// </summary>
        void NewDocument();

        /// <summary>
        /// 未保存の変更がある場合、ユーザーに保存するかどうかを確認するダイアログを表示します。
        /// </summary>
        /// <returns>ユーザーの確認結果（保存、破棄、キャンセル）。</returns>
        SaveConfirmationResult PromptForSaveIfDirty();

        /// <summary>
        /// プレビューのHTMLコンテンツを強制的に再生成して更新します。
        /// </summary>
        void UpdatePreview();

        /// <summary>
        /// エディタの現在のカーソル位置にテキストを挿入することを要求します。
        /// </summary>
        /// <param name="text">挿入するテキスト。</param>
        void RequestInsertText(string text);

        /// <summary>
        /// 指定された表示モードにフォーカスすることを要求します。
        /// </summary>
        /// <param name="mode">フォーカスするモード。</param>
        void RequestFocus(DisplayMode mode);

        /// <summary>
        /// 指定された見出しへスクロールすることを要求します。
        /// </summary>
        /// <param name="item">スクロール先の見出しアイテム。</param>
        void RequestScrollToHeader(TocItem item);

        /// <summary>
        /// テキスト挿入が要求されたときに発生します。View側で購読して実際の挿入処理を行います。
        /// </summary>
        event EventHandler<string> TextInsertionRequested;

        /// <summary>
        /// フォーカスが要求されたときに発生します。
        /// </summary>
        event EventHandler<DisplayMode> FocusRequested;

        /// <summary>
        /// 見出しへのスクロールが要求されたときに発生します。
        /// </summary>
        event EventHandler<TocItem> ScrollToHeaderRequested;
    }
}
