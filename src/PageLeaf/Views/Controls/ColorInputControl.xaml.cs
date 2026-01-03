using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PageLeaf.Views.Controls
{
    /// <summary>
    /// 色入力を提供するユーザーコントロールです。
    /// テキスト入力、プレビュー、および色選択ダイアログの呼び出しをサポートします。
    /// </summary>
    public partial class ColorInputControl : UserControl
    {
        /// <summary>
        /// <see cref="ColorInputControl"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public ColorInputControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ユーザーが入力または選択した色を表す文字列を取得または設定します。
        /// </summary>
        public string ColorText
        {
            get => (string)GetValue(ColorTextProperty);
            set => SetValue(ColorTextProperty, value);
        }

        /// <summary>
        /// ColorText 依存関係プロパティを定義します。
        /// </summary>
        public static readonly DependencyProperty ColorTextProperty =
            DependencyProperty.Register(nameof(ColorText), typeof(string), typeof(ColorInputControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 色選択ボタンがクリックされたときに実行されるコマンドを取得または設定します。
        /// </summary>
        public ICommand SelectColorCommand
        {
            get => (ICommand)GetValue(SelectColorCommandProperty);
            set => SetValue(SelectColorCommandProperty, value);
        }

        /// <summary>
        /// SelectColorCommand 依存関係プロパティを定義します。
        /// </summary>
        public static readonly DependencyProperty SelectColorCommandProperty =
            DependencyProperty.Register(nameof(SelectColorCommand), typeof(ICommand), typeof(ColorInputControl),
                new PropertyMetadata(null));

        /// <summary>
        /// コマンドに渡されるパラメータを取得または設定します。
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// CommandParameter 依存関係プロパティを定義します。
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ColorInputControl),
                new PropertyMetadata(null));
    }
}
