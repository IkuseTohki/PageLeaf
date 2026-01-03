using System.Windows;
using PageLeaf.ViewModels;

namespace PageLeaf.Views
{
    /// <summary>
    /// 致命的な例外情報を表示し、クリップボードへのコピー機能を提供するウィンドウ。
    /// </summary>
    public partial class ErrorWindow : Window
    {
        /// <summary>
        /// <see cref="ErrorWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="viewModel">ウィンドウにバインドする ViewModel。</param>
        public ErrorWindow(ErrorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
