using PageLeaf.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ICSharpCode.AvalonEdit;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// Markdownエディタ（TextBox / AvalonEdit）に対して、オートインデントなどの編集支援機能を提供する添付ビヘイビアです。
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

        public static readonly DependencyProperty ToggleDisplayModeCommandProperty =
            DependencyProperty.RegisterAttached(
                "ToggleDisplayModeCommand",
                typeof(ICommand),
                typeof(MarkdownEditorBehavior),
                new PropertyMetadata(null));

        public static ICommand GetToggleDisplayModeCommand(DependencyObject obj) => (ICommand)obj.GetValue(ToggleDisplayModeCommandProperty);
        public static void SetToggleDisplayModeCommand(DependencyObject obj, ICommand value) => obj.SetValue(ToggleDisplayModeCommandProperty, value);

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
            else if (d is TextEditor editor)
            {
                if ((bool)e.NewValue)
                {
                    editor.PreviewKeyDown += OnPreviewKeyDown;
                    editor.PreviewTextInput += OnPreviewTextInput;
                }
                else
                {
                    editor.PreviewKeyDown -= OnPreviewKeyDown;
                    editor.PreviewTextInput -= OnPreviewTextInput;
                }
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text)) return;
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            char input = e.Text[0];
            var pair = service.GetPairCharacter(input);

            if (sender is TextBox textBox)
            {
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
                        textBox.CaretIndex = caretIndex + 1;
                    }
                }
            }
            else if (sender is TextEditor editor)
            {
                if (pair.HasValue)
                {
                    e.Handled = true;
                    if (editor.SelectionLength > 0)
                    {
                        var selectedText = editor.SelectedText;
                        var start = editor.SelectionStart;
                        editor.Document.Replace(start, editor.SelectionLength, input.ToString() + selectedText + pair.Value.ToString());
                        editor.Select(start + 1, selectedText.Length);
                    }
                    else
                    {
                        int caretOffset = editor.CaretOffset;
                        editor.Document.Insert(caretOffset, input.ToString() + pair.Value.ToString());
                        editor.CaretOffset = caretOffset + 1;
                    }
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Shift) &&
                (key == Key.Left || key == Key.Right))
            {
                var command = GetToggleDisplayModeCommand((DependencyObject)sender);
                if (command != null && command.CanExecute(null))
                {
                    e.Handled = true;
                    command.Execute(null);
                    return;
                }
            }

            if (sender is TextBox textBox)
            {
                HandleTextBoxKeyDown(textBox, e);
            }
            else if (sender is TextEditor editor)
            {
                HandleTextEditorKeyDown(editor, e);
            }
        }

        #region TextBox Handlers
        private static void HandleTextBoxKeyDown(TextBox textBox, KeyEventArgs e)
        {
            // Enterキーの処理
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) HandleShiftEnter(textBox, e);
                else HandleEnterKey(textBox, e);
            }
            // Tabキーの処理
            else if (e.Key == Key.Tab) HandleTabKey(textBox, e);
            // Ctrl + V (貼り付け時のテーブル変換)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V) HandlePaste(textBox, e);
            // Ctrl + B (太字)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.B) { e.Handled = true; SurroundSelection(textBox, "**"); }
            // Ctrl + I (イタリック)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I) { e.Handled = true; SurroundSelection(textBox, "*"); }
            // Ctrl + Shift + X (打ち消し線)
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.X) { e.Handled = true; SurroundSelection(textBox, "~~"); }
            // Ctrl + Shift + C (インラインコード)
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C) { e.Handled = true; SurroundSelection(textBox, "`"); }
            // Ctrl + K (リンク)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.K) HandleLink(textBox, e);
            // Ctrl + L (タスクリスト切替)
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L) HandleToggleTaskList(textBox, e);
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

            // テーブル編集支援
            if (currentLine.Trim().StartsWith("|"))
            {
                string lineWithoutBreaks = currentLine.TrimEnd('\r', '\n');
                var formatted = service.FormatTableLine(lineWithoutBreaks);
                if (formatted != lineWithoutBreaks)
                {
                    textBox.Select(lineStart, lineWithoutBreaks.Length);
                    textBox.SelectedText = formatted;
                    lineWithoutBreaks = formatted;
                }

                int lineOffset = textBox.CaretIndex - lineStart;
                int nextOffset;
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                    nextOffset = service.GetPreviousCellOffset(lineWithoutBreaks, lineOffset);
                else
                    nextOffset = service.GetNextCellOffset(lineWithoutBreaks, lineOffset);

                textBox.CaretIndex = lineStart + nextOffset;

                int nextPipe = lineWithoutBreaks.IndexOf('|', nextOffset);
                if (nextPipe != -1)
                {
                    int start = lineStart + nextOffset;
                    int len = nextPipe - nextOffset;
                    if (nextOffset < lineWithoutBreaks.Length && lineWithoutBreaks[nextOffset] == ' ') { start++; len--; }
                    if (len > 0 && nextOffset + len - 1 < lineWithoutBreaks.Length && lineWithoutBreaks[nextOffset + len - 1] == ' ') len--;
                    if (len > 0) textBox.Select(start, len);
                }
                return;
            }

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

            // Service から挿入文字列を取得
            var insertText = service.GetShiftEnterInsertion();

            int caretIndex = textBox.CaretIndex;
            textBox.SelectedText = insertText;

            // カーソルを挿入後の位置へ
            textBox.CaretIndex = caretIndex + insertText.Length;
            textBox.SelectionLength = 0;
        }

        private static void HandleEnterKey(TextBox textBox, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            // サービスの取得
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            // 現在の行を取得
            int caretIndex = textBox.CaretIndex;
            int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
            int lineStart = textBox.GetCharacterIndexFromLineIndex(lineIndex);
            string currentLine = textBox.GetLineText(lineIndex);

            // テーブル行であれば整形
            if (currentLine.Trim().StartsWith("|"))
            {
                string lineWithoutBreaks = currentLine.TrimEnd('\r', '\n');
                var formatted = service.FormatTableLine(lineWithoutBreaks);
                if (formatted != lineWithoutBreaks)
                {
                    textBox.Select(lineStart, lineWithoutBreaks.Length);
                    textBox.SelectedText = formatted;
                    currentLine = formatted + currentLine.Substring(lineWithoutBreaks.Length);
                }
            }

            var indent = service.GetAutoIndent(currentLine);

            // コードブロック開始の判定
            if (service.IsCodeBlockStart(currentLine))
            {
                e.Handled = true;

                // Service から補完文字列を取得
                var closingText = service.GetCodeBlockCompletion(indent);
                textBox.SelectedText = closingText;

                // カーソルを中間の行に移動
                // 注: Behavior は「移動先」を知る必要があるが、それは挿入テキストの構造に依存する。
                // 厳密にはこれも Service から取得すべきだが、ここでは文字列の構造から計算する。
                // 挿入文字列は "\r\n" + indent + "\r\n" + ...
                var firstLineBreak = "\r\n" + indent;
                textBox.CaretIndex = textBox.SelectionStart + firstLineBreak.Length;
                textBox.SelectionLength = 0;
                return;
            }

            int lineOffset = caretIndex - lineStart;

            // リスト継続判定
            if (service.ShouldAutoContinueList(currentLine, lineOffset))
            {
                var listMarker = service.GetAutoListMarker(currentLine);
                if (listMarker != null)
                {
                    e.Handled = true;
                    if (listMarker == string.Empty)
                    {
                        // リスト終了：現在の行の記号を削除して改行のみにする
                        textBox.Select(lineStart, caretIndex - lineStart);
                        textBox.SelectedText = "";
                        textBox.SelectedText = "\r\n";
                    }
                    else
                    {
                        // リスト継続：改行 + マーカー挿入
                        textBox.SelectedText = "\r\n" + listMarker;
                    }
                    textBox.CaretIndex = textBox.SelectionStart + textBox.SelectionLength;
                    textBox.SelectionLength = 0;
                    return;
                }
            }

            // オートインデント判定
            if (service.ShouldAutoIndent(currentLine, lineOffset))
            {
                if (!string.IsNullOrEmpty(indent))
                {
                    e.Handled = true;
                    if (lineOffset <= indent.Length)
                    {
                        // インデント付近でのEnter: 行の先頭にインデント付き改行を挿入
                        textBox.Select(lineStart, 0);
                        textBox.SelectedText = indent + "\r\n";
                    }
                    else
                    {
                        // 途中でのEnter
                        textBox.SelectedText = "\r\n" + indent;
                    }
                    textBox.CaretIndex = textBox.SelectionStart + textBox.SelectionLength;
                    textBox.SelectionLength = 0;
                }
            }
            else if (lineOffset == 0)
            {
                // 絶対行頭でのEnter
                e.Handled = true;
                textBox.SelectedText = "\r\n";
                textBox.CaretIndex = textBox.SelectionStart + textBox.SelectionLength;
                textBox.SelectionLength = 0;
            }
        }

        private static void HandleToggleTaskList(TextBox textBox, KeyEventArgs e)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            e.Handled = true;
            int caretIndex = textBox.CaretIndex;
            int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
            int lineStart = textBox.GetCharacterIndexFromLineIndex(lineIndex);
            string currentLine = textBox.GetLineText(lineIndex);

            string lineWithoutBreaks = currentLine.TrimEnd('\r', '\n');
            string newLine = service.ToggleTaskList(lineWithoutBreaks);

            textBox.Select(lineStart, currentLine.Length);
            string lineBreaks = currentLine.Substring(lineWithoutBreaks.Length);
            textBox.SelectedText = newLine + lineBreaks;
            textBox.CaretIndex = lineStart + newLine.Length;
        }
        #endregion

        #region TextEditor Handlers
        private static void HandleTextEditorKeyDown(TextEditor editor, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) HandleShiftEnter(editor, e);
                else HandleEnterKey(editor, e);
            }
            else if (e.Key == Key.Tab) HandleTabKey(editor, e);
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V) HandlePaste(editor, e);
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.B) { e.Handled = true; SurroundSelection(editor, "**"); }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I) { e.Handled = true; SurroundSelection(editor, "*"); }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.X) { e.Handled = true; SurroundSelection(editor, "~~"); }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C) { e.Handled = true; SurroundSelection(editor, "`"); }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.K) HandleLink(editor, e);
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L) HandleToggleTaskList(editor, e);
            else if (Keyboard.Modifiers == ModifierKeys.Control &&
                     ((e.Key >= Key.D1 && e.Key <= Key.D6) || (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad6)))
            {
                int level = (e.Key >= Key.NumPad1) ? (e.Key - Key.NumPad1 + 1) : (e.Key - Key.D1 + 1);
                HandleHeading(editor, level);
                e.Handled = true;
            }
        }

        private static void HandleHeading(TextEditor editor, int level)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var currentLine = editor.Document.GetText(line);

            string newLine = service.ToggleHeading(currentLine, level);

            editor.Document.Replace(line.Offset, line.Length, newLine);
            editor.CaretOffset = line.Offset + newLine.Length;
        }

        private static void HandleLink(TextEditor editor, KeyEventArgs e)
        {
            e.Handled = true;
            if (editor.SelectionLength > 0)
            {
                string selectedText = editor.SelectedText;
                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, $"[{selectedText}]()");
                editor.CaretOffset -= 1;
            }
            else
            {
                editor.Document.Insert(editor.CaretOffset, "[]()");
                editor.CaretOffset -= 3;
            }
        }

        private static void SurroundSelection(TextEditor editor, string marker)
        {
            string selectedText = editor.SelectedText;
            int selStart = editor.SelectionStart;
            int selLen = editor.SelectionLength;

            if (selLen > 0)
            {
                editor.Document.Replace(selStart, selLen, marker + selectedText + marker);
                editor.Select(selStart + marker.Length, selLen);
            }
            else
            {
                editor.Document.Insert(editor.CaretOffset, marker + marker);
                editor.CaretOffset -= marker.Length;
            }
        }

        private static void HandlePaste(TextEditor editor, KeyEventArgs e)
        {
            if (!Clipboard.ContainsText()) return;
            string text = Clipboard.GetText();
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            var markdownTable = service.ConvertToMarkdownTable(text);
            if (markdownTable != null)
            {
                e.Handled = true;
                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, markdownTable);
            }
        }

        private static void HandleTabKey(TextEditor editor, KeyEventArgs e)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            var settingsService = App.AppHost?.Services.GetService<ISettingsService>();
            if (service == null || settingsService == null) return;
            var settings = settingsService.CurrentSettings;
            e.Handled = true;

            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var currentLine = editor.Document.GetText(line);

            // テーブル編集支援
            if (currentLine.Trim().StartsWith("|"))
            {
                // 現在の行を整形
                var formatted = service.FormatTableLine(currentLine);
                if (formatted != currentLine)
                {
                    editor.Document.Replace(line.Offset, line.Length, formatted);
                    currentLine = formatted;
                }

                // セル移動
                int lineOffset = editor.CaretOffset - line.Offset;
                int nextOffset;
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                    nextOffset = service.GetPreviousCellOffset(currentLine, lineOffset);
                else
                    nextOffset = service.GetNextCellOffset(currentLine, lineOffset);

                editor.CaretOffset = line.Offset + nextOffset;

                // セル内容を選択
                int nextPipe = currentLine.IndexOf('|', nextOffset);
                if (nextPipe != -1)
                {
                    int selectionLen = nextPipe - nextOffset;
                    if (selectionLen > 0)
                    {
                        int start = line.Offset + nextOffset;
                        int len = selectionLen;
                        // 前後のスペースを考慮して選択範囲を調整
                        if (nextOffset < currentLine.Length && currentLine[nextOffset] == ' ') { start++; len--; }
                        if (len > 0 && nextOffset + selectionLen - 1 < currentLine.Length && currentLine[nextOffset + selectionLen - 1] == ' ') len--;

                        if (len > 0) editor.Select(start, len);
                    }
                }
                return;
            }

            bool isListItem = service.GetAutoListMarker(currentLine) != null;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                string newLine = service.DecreaseIndent(currentLine, settings);
                if (currentLine != newLine)
                {
                    editor.Document.Replace(line.Offset, line.Length, newLine);
                }
            }
            else
            {
                if (isListItem)
                {
                    string newLine = service.IncreaseIndent(currentLine, settings);
                    editor.Document.Replace(line.Offset, line.Length, newLine);
                }
                else
                {
                    string indent = service.GetIndentString(settings);
                    editor.Document.Insert(editor.CaretOffset, indent);
                }
            }
        }

        private static void HandleShiftEnter(TextEditor editor, KeyEventArgs e)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;
            e.Handled = true;
            string insertText = service.GetShiftEnterInsertion();
            editor.Document.Insert(editor.CaretOffset, insertText);
        }

        private static void HandleEnterKey(TextEditor editor, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;

            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var currentLine = editor.Document.GetText(line);

            // テーブル行であれば整形する
            if (currentLine.Trim().StartsWith("|"))
            {
                var formatted = service.FormatTableLine(currentLine);
                if (formatted != currentLine)
                {
                    editor.Document.Replace(line.Offset, line.Length, formatted);
                    currentLine = formatted;
                }
            }

            var indent = service.GetAutoIndent(currentLine);

            if (service.IsCodeBlockStart(currentLine))
            {
                e.Handled = true;
                string closing = service.GetCodeBlockCompletion(indent);
                editor.Document.Insert(editor.CaretOffset, closing);
                // Move cursor to middle line
                editor.CaretOffset = line.EndOffset + Environment.NewLine.Length + indent.Length;
                return;
            }

            int lineOffset = editor.CaretOffset - line.Offset;

            // リスト継続判定
            if (service.ShouldAutoContinueList(currentLine, lineOffset))
            {
                var listMarker = service.GetAutoListMarker(currentLine);
                if (listMarker != null)
                {
                    e.Handled = true;
                    if (listMarker == string.Empty)
                    {
                        editor.Document.Replace(line.Offset, line.Length, "");
                    }
                    else
                    {
                        editor.Document.Insert(editor.CaretOffset, Environment.NewLine + listMarker);
                    }
                }
                return;
            }

            // オートインデント判定
            if (service.ShouldAutoIndent(currentLine, lineOffset))
            {
                if (!string.IsNullOrEmpty(indent))
                {
                    e.Handled = true;
                    if (lineOffset <= indent.Length)
                    {
                        // インデント内でのEnter: 行の先頭に「インデント + 改行」を挿入してインデント付き空行を上に作る
                        editor.Document.Insert(line.Offset, indent + Environment.NewLine);
                    }
                    else
                    {
                        // コンテンツ途中でのEnter: 通常のオートインデント
                        editor.Document.Insert(editor.CaretOffset, Environment.NewLine + indent);
                    }
                }
            }
            else if (lineOffset == 0)
            {
                // 絶対行頭でのEnter: インデントを引き継がず、純粋な改行のみを挿入する
                e.Handled = true;
                editor.Document.Insert(editor.CaretOffset, Environment.NewLine);
            }
        }

        private static void HandleToggleTaskList(TextEditor editor, KeyEventArgs e)
        {
            var service = App.AppHost?.Services.GetService<IEditingSupportService>();
            if (service == null) return;
            e.Handled = true;

            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var currentLine = editor.Document.GetText(line);
            string newLine = service.ToggleTaskList(currentLine);

            editor.Document.Replace(line.Offset, line.Length, newLine);
            editor.CaretOffset = line.Offset + newLine.Length;
        }
        #endregion
    }
}
