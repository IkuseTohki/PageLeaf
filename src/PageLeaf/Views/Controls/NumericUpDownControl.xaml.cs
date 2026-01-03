using System;
using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Views.Controls
{
    /// <summary>
    /// 数値入力と単位表示を組み合わせたユーザーコントロールです。
    /// 上下ボタンによる数値の増減をサポートします。
    /// </summary>
    public partial class NumericUpDownControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericUpDownControl),
                new PropertyMetadata("px"));

        public static readonly DependencyProperty IsUnitVisibleProperty =
            DependencyProperty.Register(nameof(IsUnitVisible), typeof(bool), typeof(NumericUpDownControl),
                new PropertyMetadata(true));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public bool IsUnitVisible
        {
            get => (bool)GetValue(IsUnitVisibleProperty);
            set => SetValue(IsUnitVisibleProperty, value);
        }

        public NumericUpDownControl()
        {
            InitializeComponent();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustValue(1);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustValue(-1);
        }

        private void AdjustValue(int direction)
        {
            if (double.TryParse(Value, out double current))
            {
                double step = 1.0;
                double newValue = current + (step * direction);
                if (newValue < 0) newValue = 0;
                Value = Math.Round(newValue, 2).ToString();
            }
            else if (string.IsNullOrEmpty(Value))
            {
                Value = direction > 0 ? "1" : "0";
            }
        }
    }
}
