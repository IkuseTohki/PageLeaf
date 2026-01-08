using PageLeaf.ViewModels;
using System.Windows;

namespace PageLeaf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.RequestScrollToHeader += ViewModel_RequestScrollToHeader;
            viewModel.RequestFocus += ViewModel_RequestFocus;
        }

        private void ViewModel_RequestFocus(object? sender, PageLeaf.Models.DisplayMode mode)
        {
            if (mode == PageLeaf.Models.DisplayMode.Markdown)
            {
                MarkdownEditor.Focus();
            }
            else if (mode == PageLeaf.Models.DisplayMode.Viewer)
            {
                MarkdownViewer.Focus();
            }
        }

        private async void ViewModel_RequestScrollToHeader(object? sender, PageLeaf.Models.TocItem item)
        {
            if (DataContext is MainViewModel viewModel)
            {
                if (viewModel.Editor.SelectedMode == PageLeaf.Models.DisplayMode.Markdown)
                {
                    // エディタモード: 行番号へスクロール
                    if (item.LineNumber >= 0 && item.LineNumber < MarkdownEditor.LineCount)
                    {
                        var charIndex = MarkdownEditor.GetCharacterIndexFromLineIndex(item.LineNumber);
                        MarkdownEditor.Focus();
                        MarkdownEditor.Select(charIndex, 0);

                        // 垂直方向の真ん中にくるようにスクロール位置を調整
                        var rect = MarkdownEditor.GetRectFromCharacterIndex(charIndex);
                        if (!rect.IsEmpty)
                        {
                            var verticalOffset = MarkdownEditor.VerticalOffset + rect.Top - (MarkdownEditor.ViewportHeight / 2);
                            MarkdownEditor.ScrollToVerticalOffset(verticalOffset);
                        }
                    }
                }
                else
                {
                    // ビューワーモード: IDへスクロール
                    if (!string.IsNullOrEmpty(item.Id) && MarkdownViewer != null && MarkdownViewer.CoreWebView2 != null)
                    {
                        await MarkdownViewer.ExecuteScriptAsync($"document.getElementById('{item.Id}').scrollIntoView();");
                    }
                }
            }
        }
    }
}
