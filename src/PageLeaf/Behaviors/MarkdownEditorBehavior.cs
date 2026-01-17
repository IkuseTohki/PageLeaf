using PageLeaf.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// Markdownエディタ（TextBox）に対して、オートインデントなどの編集支援機能を提供する添付ビヘイビアです。
    /// </summary>
    public static class MarkdownEditorBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(MarkdownEditorBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.PreviewKeyDown += OnPreviewKeyDown;
                    textBox.PreviewTextInput += OnPreviewTextInput;
                }
                else
                {
                    textBox.PreviewKeyDown -= OnPreviewKeyDown;
                    textBox.PreviewTextInput -= OnPreviewTextInput;
                }
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;
            if (string.IsNullOrEmpty(e.Text)) return;

            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            char input = e.Text[0];
            var pair = service.GetPairCharacter(input);

            if (pair.HasValue)
            {
                // 選択範囲がある場合は「選択範囲の囲み」機能
                if (textBox.SelectionLength > 0)
                {
                    e.Handled = true;
                    var selectedText = textBox.SelectedText;
                    var start = textBox.SelectionStart;

                    textBox.SelectedText = input.ToString() + selectedText + pair.Value.ToString();

                    // 囲まれたテキストを選択状態にする
                    textBox.Select(start + 1, selectedText.Length);
                }
                // 選択範囲がない場合は単純な挿入
                else
                {
                    e.Handled = true;
                    int caretIndex = textBox.CaretIndex;
                    textBox.SelectedText = input.ToString() + pair.Value.ToString();
                    textBox.CaretIndex = caretIndex + 1; // 間に置く
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox textBox)) return;

            // Enterキーの処理
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    HandleShiftEnter(textBox, e);
                }
                else
                {
                    HandleEnterKey(textBox, e);
                }
            }
            // Tabキーの処理
            else if (e.Key == Key.Tab)
            {
                HandleTabKey(textBox, e);
            }
            // Ctrl + V (貼り付け時のテーブル変換)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                HandlePaste(textBox, e);
            }
            // Ctrl + B (太字)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.B)
            {
                e.Handled = true;
                SurroundSelection(textBox, "**");
            }
            // Ctrl + I (イタリック)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I)
            {
                e.Handled = true;
                SurroundSelection(textBox, "*");
            }
            // Ctrl + Shift + X (打ち消し線)
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.X)
            {
                e.Handled = true;
                SurroundSelection(textBox, "~~");
            }
            // Ctrl + Shift + C (インラインコード)
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C)
            {
                e.Handled = true;
                SurroundSelection(textBox, "`");
            }
            // Ctrl + K (リンク)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.K)
            {
                HandleLink(textBox, e);
            }
            // Ctrl + 1~6 (見出し)
            else if (Keyboard.Modifiers == ModifierKeys.Control &&
                     ((e.Key >= Key.D1 && e.Key <= Key.D6) || (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad6)))
            {
                int level = (e.Key >= Key.NumPad1) ? (e.Key - Key.NumPad1 + 1) : (e.Key - Key.D1 + 1);
                HandleHeading(textBox, level);
                e.Handled = true;
            }
        }

        private static void HandleHeading(TextBox textBox, int level)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            int caretIndex = textBox.CaretIndex;
            int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
            int lineStart = textBox.GetCharacterIndexFromLineIndex(lineIndex);
            string currentLine = textBox.GetLineText(lineIndex);

            // 改行コード（\r\n）が含まれる場合は取り除く（GetLineTextは含む場合がある）
            string lineWithoutBreaks = currentLine.TrimEnd('\r', '\n');
            string newLine = service.ToggleHeading(lineWithoutBreaks, level);

            textBox.Select(lineStart, currentLine.Length);
            // 改行を維持するために、元の行に含まれていた改行を付与し直す
            string lineBreaks = currentLine.Substring(lineWithoutBreaks.Length);
            textBox.SelectedText = newLine + lineBreaks;

            // カーソル位置の調整
            textBox.CaretIndex = lineStart + newLine.Length;
            textBox.SelectionLength = 0;
        }

        private static void HandleLink(TextBox textBox, KeyEventArgs e)
        {
            e.Handled = true;
            var selectedText = textBox.SelectedText;
            var start = textBox.SelectionStart;

            if (textBox.SelectionLength > 0)
            {
                // [選択範囲]() を作成
                textBox.SelectedText = "[" + selectedText + "]()";
                // URL入力位置（)の直前）にカーソルを移動
                textBox.CaretIndex = start + selectedText.Length + 3;
            }
            else
            {
                // []() を挿入して [] の中にカーソル移動
                textBox.SelectedText = "[]()";
                textBox.CaretIndex = start + 1;
            }
        }

        /// <summary>
        /// 選択範囲を指定されたマーカーで囲みます。選択範囲がない場合はマーカーのペアを挿入します。
        /// </summary>
        private static void SurroundSelection(TextBox textBox, string marker)
        {
            var selectedText = textBox.SelectedText;
            var start = textBox.SelectionStart;
            var markerLen = marker.Length;

            if (textBox.SelectionLength > 0)
            {
                // 選択範囲をマーカーで囲む
                textBox.SelectedText = marker + selectedText + marker;
                // 囲まれたテキストを再度選択状態にする
                textBox.Select(start + markerLen, selectedText.Length);
            }
            else
            {
                // 選択範囲がない場合はペアを挿入して中央にカーソル移動
                textBox.SelectedText = marker + marker;
                textBox.CaretIndex = start + markerLen;
            }
        }

        private static void HandlePaste(TextBox textBox, KeyEventArgs e)

        {
            // クリップボードのテキストを取得
            if (!Clipboard.ContainsText()) return;
            string text = Clipboard.GetText();

            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            // テーブル変換を試みる
            var markdownTable = service.ConvertToMarkdownTable(text);
            if (markdownTable != null)
            {
                e.Handled = true;
                textBox.SelectedText = markdownTable;
                textBox.SelectionLength = 0;
                textBox.CaretIndex = textBox.SelectionStart + markdownTable.Length;
            }
            // テーブル変換できなければ標準の貼り付け動作に任せる (e.Handled = false のまま)
        }

        private static void HandleTabKey(TextBox textBox, KeyEventArgs e)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            var settingsService = App.AppHost?.Services.GetService<ISettingsService>();
            if (service == null || settingsService == null) return;

            var settings = settingsService.CurrentSettings;
            e.Handled = true;

            int caretIndex = textBox.CaretIndex;
            int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
            int lineStart = textBox.GetCharacterIndexFromLineIndex(lineIndex);
            string currentLine = textBox.GetLineText(lineIndex);

            // リストアイテムかどうか判定
            bool isListItem = service.GetAutoListMarker(currentLine) != null;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Shift + Tab: アウトデント
                string newLine = service.DecreaseIndent(currentLine, settings);
                if (currentLine != newLine)
                {
                    textBox.Select(lineStart, currentLine.Length);
                    textBox.SelectedText = newLine;

                    // カーソル位置の調整
                    int removedCount = currentLine.Length - newLine.Length;
                    textBox.CaretIndex = Math.Max(lineStart, caretIndex - removedCount);
                    textBox.SelectionLength = 0;
                }
            }
            else
            {
                // Tab: インデント
                if (isListItem)
                {
                    // リストアイテムなら行頭にインデント追加
                    string newLine = service.IncreaseIndent(currentLine, settings);
                    textBox.Select(lineStart, currentLine.Length);
                    textBox.SelectedText = newLine;

                    // カーソル位置の調整（インデント分進める）
                    int addedCount = newLine.Length - currentLine.Length;
                    textBox.CaretIndex = caretIndex + addedCount;
                    textBox.SelectionLength = 0;
                }
                else
                {
                    // リストでなければ現在の位置にインデント挿入
                    var indent = service.GetIndentString(settings);
                    textBox.SelectedText = indent;
                    textBox.CaretIndex = caretIndex + indent.Length;
                    textBox.SelectionLength = 0;
                }
            }
        }

        private static void HandleShiftEnter(TextBox textBox, KeyEventArgs e)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            e.Handled = true;

            // 改ページ用タグの取得
            var pageBreak = service.GetPageBreakString();

            // 視認性を考慮し、後に改行を入れて挿入する
            var insertText = pageBreak + "\r\n";

            int caretIndex = textBox.CaretIndex;
            textBox.SelectedText = insertText;

            // カーソルを挿入後の位置へ
            textBox.CaretIndex = caretIndex + insertText.Length;
            textBox.SelectionLength = 0;
        }

        private static void HandleEnterKey(TextBox textBox, KeyEventArgs e)
        {

            // 修飾キーが押されている場合は標準動作
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            // サービスの取得
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            // 現在の行を取得
            int caretIndex = textBox.CaretIndex;
            int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
            string currentLine = textBox.GetLineText(lineIndex);

            var indent = service.GetAutoIndent(currentLine);

            // コードブロック開始の判定
            if (service.IsCodeBlockStart(currentLine))
            {
                e.Handled = true;

                // 開始行の次に行を挿入し、さらにその次に閉じ記号を挿入
                // 構成: \r\n (インデント) \r\n (インデント) ```
                var closingText = "\r\n" + indent + "\r\n" + indent + "```";
                textBox.SelectedText = closingText;

                // カーソルを中間の行に移動
                textBox.CaretIndex = textBox.SelectionStart + ("\r\n" + indent).Length;
                textBox.SelectionLength = 0;
                return;
            }

            // リストマーカーの取得
            var listMarker = service.GetAutoListMarker(currentLine);

            if (listMarker != null)
            {
                e.Handled = true;

                if (listMarker == string.Empty)
                {
                    // リスト終了：現在の行の記号を削除して改行のみにする
                    int lineStart = textBox.GetCharacterIndexFromLineIndex(lineIndex);
                    textBox.Select(lineStart, caretIndex - lineStart);
                    textBox.SelectedText = "";
                    textBox.SelectedText = "\r\n";
                }
                else
                {
                    // リスト継続：改行 + マーカー挿入
                    textBox.SelectedText = "\r\n" + listMarker;
                }

                // カーソルを挿入されたテキストの末尾に移動
                textBox.CaretIndex = textBox.SelectionStart + textBox.SelectionLength;
                textBox.SelectionLength = 0;
                return;
            }

            if (string.IsNullOrEmpty(indent)) return;

            // Enterの標準動作を抑制し、自前で改行 + インデントを挿入
            e.Handled = true;

            // 選択範囲がある場合は改行で置換
            var insertionText = "\r\n" + indent;
            textBox.SelectedText = insertionText;

            // カーソル位置を更新
            textBox.CaretIndex = textBox.SelectionStart + textBox.SelectionLength;
            textBox.SelectionLength = 0;
        }
    }
}

