using System.Windows;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// 複数のスタイルをマージするための添付ビヘイビアです。
    /// BasedOn プロパティだけでは1つのスタイルしか継承できない制約を回避するために使用します。
    /// </summary>
    public static class StyleBehavior
    {
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.RegisterAttached("IsDirty", typeof(bool), typeof(StyleBehavior), new PropertyMetadata(false));

        public static bool GetIsDirty(DependencyObject obj) => (bool)obj.GetValue(IsDirtyProperty);
        public static void SetIsDirty(DependencyObject obj, bool value) => obj.SetValue(IsDirtyProperty, value);

        public static readonly DependencyProperty MergeStyleProperty =
            DependencyProperty.RegisterAttached("MergeStyle", typeof(Style), typeof(StyleBehavior), new PropertyMetadata(null, OnMergeStyleChanged));

        public static Style GetMergeStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(MergeStyleProperty);
        }

        public static void SetMergeStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(MergeStyleProperty, value);
        }

        private static void OnMergeStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && e.NewValue is Style mergeStyle)
            {
                // 現在のスタイル（XAMLでStyle="{StaticResource ...}"と指定されているもの）を取得
                var currentStyle = element.Style;

                if (currentStyle != null)
                {
                    // 新しいスタイルを作成し、現在のスタイルをベースにする
                    var newStyle = new Style(element.GetType(), currentStyle);

                    // マージするスタイルのセッターをすべてコピーする
                    foreach (var setter in mergeStyle.Setters)
                    {
                        newStyle.Setters.Add(setter);
                    }

                    // トリガーもコピー
                    foreach (var trigger in mergeStyle.Triggers)
                    {
                        newStyle.Triggers.Add(trigger);
                    }

                    element.Style = newStyle;
                }
                else
                {
                    // 現在のスタイルがない場合は、マージするスタイルをそのまま適用
                    element.Style = mergeStyle;
                }
            }
        }
    }
}
