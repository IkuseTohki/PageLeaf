using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PageLeaf.Utilities;

namespace PageLeaf.Views.Controls
{
    /// <summary>
    /// ColorInputControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorInputControl : UserControl
    {
        public ColorInputControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ユーザーが入力または選択した色を表す文字列を取得または設定します。
        /// これは依存関係プロパティです。
        /// </summary>
        public string ColorText
        {
            get { return (string)GetValue(ColorTextProperty); }
            set { SetValue(ColorTextProperty, value); }
        }

        public static readonly DependencyProperty ColorTextProperty =
            DependencyProperty.Register("ColorText", typeof(string), typeof(ColorInputControl), 
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 色選択ボタンがクリックされたときに呼び出されます。
        /// カラーダイアログを表示し、選択された色でColorTextプロパティを更新します。
        /// </summary>
        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            // 現在の色をダイアログの初期色として設定
            if (!string.IsNullOrEmpty(ColorText))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(ColorText);
                    colorDialog.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                catch
                {
                    // 無視
                }
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                ColorText = ColorConverterHelper.ToRgbString(selectedColor);
            }
        }
    }
}
