using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Utilities
{
    public static class GridSplitterBehavior
    {
        #region AssociatedWidth Attached Property

        /// <summary>
        /// ViewModelのプロパティとGridのColumnDefinitionの幅を双方向でバインドするための添付プロパティ。
        /// </summary>
        public static readonly DependencyProperty AssociatedWidthProperty =
            DependencyProperty.RegisterAttached(
                "AssociatedWidth",
                typeof(GridLength),
                typeof(GridSplitterBehavior),
                new FrameworkPropertyMetadata(
                    GridLength.Auto,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnAssociatedWidthChanged));

        public static GridLength GetAssociatedWidth(DependencyObject obj)
        {
            return (GridLength)obj.GetValue(AssociatedWidthProperty);
        }

        public static void SetAssociatedWidth(DependencyObject obj, GridLength value)
        {
            obj.SetValue(AssociatedWidthProperty, value);
        }

        #endregion

        #region ColumnIndex Attached Property

        /// <summary>
        /// GridSplitterが影響を与える対象のColumnDefinitionのインデックスを指定するための添付プロパティ。
        /// </summary>
        public static readonly DependencyProperty ColumnIndexProperty =
            DependencyProperty.RegisterAttached(
                "ColumnIndex",
                typeof(int),
                typeof(GridSplitterBehavior),
                new PropertyMetadata(-1));

        public static int GetColumnIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnIndexProperty);
        }

        public static void SetColumnIndex(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnIndexProperty, value);
        }

        #endregion

        #region EnableTwoWayBinding Attached Property

        /// <summary>
        /// このビヘイビアを有効にするためのトリガーとなる添付プロパティ。
        /// </summary>
        public static readonly DependencyProperty EnableTwoWayBindingProperty =
            DependencyProperty.RegisterAttached(
                "EnableTwoWayBinding",
                typeof(bool),
                typeof(GridSplitterBehavior),
                new PropertyMetadata(false, OnEnableTwoWayBindingChanged));

        public static bool GetEnableTwoWayBinding(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableTwoWayBindingProperty);
        }

        public static void SetEnableTwoWayBinding(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableTwoWayBindingProperty, value);
        }

        #endregion

        /// <summary>
        /// EnableTwoWayBindingプロパティが変更されたときに呼び出され、イベントの購読・解除を行う。
        /// </summary>
        private static void OnEnableTwoWayBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridSplitter splitter)
            {
                if ((bool)e.NewValue)
                {
                    splitter.DragCompleted += OnDragCompleted;
                    splitter.Unloaded += OnSplitterUnloaded;
                }
                else
                {
                    splitter.DragCompleted -= OnDragCompleted;
                    splitter.Unloaded -= OnSplitterUnloaded;
                }
            }
        }

        /// <summary>
        /// ViewModelからAssociatedWidthが変更されたときに呼び出され、ColumnDefinitionの幅を更新する。
        /// </summary>
        private static void OnAssociatedWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridSplitter splitter)
            {
                int columnIndex = GetColumnIndex(splitter);
                if (splitter.Parent is Grid grid && columnIndex >= 0 && grid.ColumnDefinitions.Count > columnIndex)
                {
                    // ViewModelからの変更をカラムの幅に反映
                    grid.ColumnDefinitions[columnIndex].Width = (GridLength)e.NewValue;
                }
            }
        }

        /// <summary>
        /// ユーザーがGridSplitterをドラッグしたときに呼び出され、ViewModelのプロパティを更新する。
        /// </summary>
        private static void OnDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (sender is GridSplitter splitter)
            {
                int columnIndex = GetColumnIndex(splitter);
                if (splitter.Parent is Grid grid && columnIndex >= 0 && grid.ColumnDefinitions.Count > columnIndex)
                {
                    // TwoWayバインディングを通じてViewModelのプロパティを更新
                    SetAssociatedWidth(splitter, grid.ColumnDefinitions[columnIndex].Width);
                }
            }
        }

        /// <summary>
        /// GridSplitterがアンロードされるときにイベントハンドラをクリーンアップする。
        /// </summary>
        private static void OnSplitterUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is GridSplitter splitter)
            {
                splitter.DragCompleted -= OnDragCompleted;
                splitter.Unloaded -= OnSplitterUnloaded;
            }
        }
    }
}
