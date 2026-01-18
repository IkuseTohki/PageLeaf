using PageLeaf.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Views.Controls
{
    public partial class CssNumericEditorControl : UserControl
    {
        public static readonly DependencyProperty CssValueProperty =
            DependencyProperty.Register(nameof(CssValue), typeof(string), typeof(CssNumericEditorControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCssValueChanged));

        public string? CssValue
        {
            get => (string?)GetValue(CssValueProperty);
            set => SetValue(CssValueProperty, value);
        }

        public static readonly DependencyProperty ShowSpinnerProperty =
            DependencyProperty.Register(nameof(ShowSpinner), typeof(bool), typeof(CssNumericEditorControl), new PropertyMetadata(false));

        public bool ShowSpinner
        {
            get => (bool)GetValue(ShowSpinnerProperty);
            set => SetValue(ShowSpinnerProperty, value);
        }

        // 内部バインディング用
        public static readonly DependencyProperty InternalValueProperty =
            DependencyProperty.Register(nameof(InternalValue), typeof(string), typeof(CssNumericEditorControl), new PropertyMetadata(null, OnInternalChanged));

        public string? InternalValue
        {
            get => (string?)GetValue(InternalValueProperty);
            set => SetValue(InternalValueProperty, value);
        }

        public static readonly DependencyProperty InternalUnitProperty =
            DependencyProperty.Register(nameof(InternalUnit), typeof(string), typeof(CssNumericEditorControl), new PropertyMetadata(null, OnInternalChanged));

        public string? InternalUnit
        {
            get => (string?)GetValue(InternalUnitProperty);
            set => SetValue(InternalUnitProperty, value);
        }

        private bool _isUpdating;

        public CssNumericEditorControl()
        {
            InitializeComponent();
        }

        private static void OnCssValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CssNumericEditorControl)d;
            if (control._isUpdating) return;

            control._isUpdating = true;
            try
            {
                var (val, unit) = UnitConversionHelper.Split(e.NewValue as string);
                control.InternalValue = val.ToString();
                control.InternalUnit = unit;
            }
            finally
            {
                control._isUpdating = false;
            }
        }

        private static void OnInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CssNumericEditorControl)d;
            if (control._isUpdating) return;

            control._isUpdating = true;
            try
            {
                var currentValText = control.InternalValue;
                var currentUnitText = control.InternalUnit;

                if (e.Property == InternalUnitProperty)
                {
                    // 単位が変わった場合は数値を変換
                    if (double.TryParse(currentValText, out var val))
                    {
                        var oldUnit = e.OldValue as string ?? "px";
                        var newUnit = e.NewValue as string ?? "px";

                        var supported = new[] { "px", "em", "%" };
                        if (supported.Contains(oldUnit) && supported.Contains(newUnit))
                        {
                            var converted = UnitConversionHelper.Convert(val, oldUnit, newUnit);
                            control.InternalValue = UnitConversionHelper.Round(converted).ToString();
                            currentValText = control.InternalValue;
                        }
                    }
                }

                // 結合して外部プロパティを更新
                if (double.TryParse(currentValText, out var num))
                {
                    control.CssValue = UnitConversionHelper.Format(num, currentUnitText ?? "px");
                }
                else
                {
                    control.CssValue = currentValText;
                }
            }
            finally
            {
                control._isUpdating = false;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) => AdjustValue(1);
        private void DownButton_Click(object sender, RoutedEventArgs e) => AdjustValue(-1);

        private void AdjustValue(double delta)
        {
            if (double.TryParse(InternalValue, out var val))
            {
                var step = (InternalUnit == "em" || InternalUnit == "rem") ? 0.1 : 1.0;
                InternalValue = (val + (delta * step)).ToString();
            }
        }
    }
}
