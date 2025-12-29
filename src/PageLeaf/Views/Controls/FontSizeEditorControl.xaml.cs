using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Views.Controls
{
    /// <summary>
    /// 数値入力と単位表示を組み合わせた、フォントサイズ編集用のユーザーコントロールです。
    /// </summary>
    public partial class FontSizeEditorControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(FontSizeEditorControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(FontSizeEditorControl),
                new PropertyMetadata("px"));

        public static readonly DependencyProperty IsUnitVisibleProperty =
            DependencyProperty.Register(nameof(IsUnitVisible), typeof(bool), typeof(FontSizeEditorControl),
                new PropertyMetadata(true));

        /// <summary>
        /// 編集中の数値文字列（単位なし）を取得または設定します。
        /// </summary>
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// 表示する単位文字列（例: "px", "em"）を取得または設定します。
        /// </summary>
        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        /// <summary>
        /// 単位ラベルを表示するかどうかを取得または設定します。
        /// </summary>
        public bool IsUnitVisible
        {
            get => (bool)GetValue(IsUnitVisibleProperty);
            set => SetValue(IsUnitVisibleProperty, value);
        }

        public FontSizeEditorControl()
        {
            InitializeComponent();
        }
    }
}
