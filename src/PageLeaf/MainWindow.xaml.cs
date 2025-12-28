using PageLeaf.ViewModels;
using System;
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

            // ウィンドウの移動やリサイズ時にPopupの位置を更新する
            this.LocationChanged += UpdatePopupPosition;
            this.SizeChanged += UpdatePopupPosition;

            // GridSplitterによる幅変更時にPopupを追従させる
            this.MainGrid.LayoutUpdated += UpdatePopupPosition;
        }

        /// <summary>
        /// Popupの位置を強制的に更新し、境界線に追従させます。
        /// </summary>
        private void UpdatePopupPosition(object? sender, EventArgs e)
        {
            if (TogglePopup.IsOpen)
            {
                // HorizontalOffsetを一時的に変更して戻すことで、位置計算を再トリガーする
                var offset = TogglePopup.HorizontalOffset;
                TogglePopup.HorizontalOffset = offset + 0.01;
                TogglePopup.HorizontalOffset = offset;
            }
        }
    }
}
