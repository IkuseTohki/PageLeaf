using System.Windows;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// ビジュアルツリー外の要素（InputBindings等）にDataContextを渡すためのプロキシクラスです。
    /// </summary>
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}
