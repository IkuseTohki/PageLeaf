using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Windows;
using System.Xml;

namespace PageLeaf.Behaviors
{
    public static class AvalonEditBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(AvalonEditBehavior),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
        public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
            {
                var newText = (string)e.NewValue;
                if (editor.Text != newText)
                {
                    editor.Text = newText ?? "";
                }
            }
        }

        public static readonly DependencyProperty EnableTextBindingProperty =
             DependencyProperty.RegisterAttached("EnableTextBinding", typeof(bool), typeof(AvalonEditBehavior),
             new PropertyMetadata(false, OnEnableTextBindingChanged));

        public static bool GetEnableTextBinding(DependencyObject obj) => (bool)obj.GetValue(EnableTextBindingProperty);
        public static void SetEnableTextBinding(DependencyObject obj, bool value) => obj.SetValue(EnableTextBindingProperty, value);

        private static void OnEnableTextBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
            {
                if ((bool)e.NewValue)
                {
                    editor.TextChanged += Editor_TextChanged;
                }
                else
                {
                    editor.TextChanged -= Editor_TextChanged;
                }
            }
        }

        private static void Editor_TextChanged(object? sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                SetText(editor, editor.Text);
            }
        }

        public static readonly DependencyProperty SyntaxHighlightingProperty =
             DependencyProperty.RegisterAttached("SyntaxHighlighting", typeof(string), typeof(AvalonEditBehavior),
             new PropertyMetadata(null, OnSyntaxHighlightingChanged));

        public static string GetSyntaxHighlighting(DependencyObject obj) => (string)obj.GetValue(SyntaxHighlightingProperty);
        public static void SetSyntaxHighlighting(DependencyObject obj, string value) => obj.SetValue(SyntaxHighlightingProperty, value);

        private static void OnSyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor && e.NewValue is string name && name == "Markdown")
            {
                try
                {
                    var uri = new Uri("pack://application:,,,/PageLeaf;component/Resources/Markdown.xshd");
                    var resource = Application.GetResourceStream(uri);
                    if (resource != null)
                    {
                        using (var stream = resource.Stream)
                        {
                            using (var reader = new XmlTextReader(stream))
                            {
                                editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore load error
                }
            }
        }
    }
}
