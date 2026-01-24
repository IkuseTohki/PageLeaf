using PageLeaf.Services;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// IEditorService のテキスト挿入要求イベントを購読し、関連付けられた TextBox にテキストを挿入する添付ビヘイビアを提供します。
    /// </summary>
    public static class EditorTextInsertionBehavior
    {
        public static readonly DependencyProperty EditorServiceProperty =
            DependencyProperty.RegisterAttached(
                "EditorService",
                typeof(IEditorService),
                typeof(EditorTextInsertionBehavior),
                new PropertyMetadata(null, OnEditorServiceChanged));

        public static IEditorService GetEditorService(DependencyObject obj) => (IEditorService)obj.GetValue(EditorServiceProperty);
        public static void SetEditorService(DependencyObject obj, IEditorService value) => obj.SetValue(EditorServiceProperty, value);

        private static void OnEditorServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                // 古いリスナーがあれば解除
                var oldListener = GetListener(control);
                if (oldListener != null)
                {
                    oldListener.Detach();
                    SetListener(control, null);
                }

                if (e.NewValue is IEditorService newService)
                {
                    var newListener = new EditorInsertionListener(control, newService);
                    SetListener(control, newListener);
                }
            }
        }

        // リスナー保持用のプライベート添付プロパティ
        private static readonly DependencyProperty ListenerProperty =
            DependencyProperty.RegisterAttached("Listener", typeof(EditorInsertionListener), typeof(EditorTextInsertionBehavior), new PropertyMetadata(null));

        private static EditorInsertionListener? GetListener(DependencyObject obj) => (EditorInsertionListener?)obj.GetValue(ListenerProperty);
        private static void SetListener(DependencyObject obj, EditorInsertionListener? value) => obj.SetValue(ListenerProperty, value);

        private class EditorInsertionListener
        {
            private readonly Control _control;
            private readonly IEditorService _service;

            public EditorInsertionListener(Control control, IEditorService service)
            {
                _control = control;
                _service = service;
                _service.TextInsertionRequested += OnTextInsertionRequested;
            }

            public void Detach()
            {
                _service.TextInsertionRequested -= OnTextInsertionRequested;
            }

            private void OnTextInsertionRequested(object? sender, string text)
            {
                if (string.IsNullOrEmpty(text)) return;

                if (_control is TextEditor editor)
                {
                    editor.TextArea.Selection.ReplaceSelectionWithText(text);
                    editor.Focus();
                }
                else if (_control is TextBox textBox)
                {
                    textBox.SelectedText = text;
                    textBox.Focus();
                    textBox.SelectionLength = 0;
                    textBox.SelectionStart += text.Length;
                }
            }
        }
    }
}
