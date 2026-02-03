using PageLeaf.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Views.Controls
{
    /// <summary>
    /// 数値と単位（px, em, %）を編集するためのエディタコントロールです。
    /// 単位の固定表示モードもサポートします。
    /// </summary>
    public partial class CssNumericEditorControl : UserControl
    {
        public ObservableCollection<string> AvailableValues { get; } = new ObservableCollection<string>();
        public string[] AllUnits { get; } = new[] { "px", "em", "%", "" };

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

        public static readonly DependencyProperty IsUnitFixedProperty =
            DependencyProperty.Register(nameof(IsUnitFixed), typeof(bool), typeof(CssNumericEditorControl), new PropertyMetadata(false));

        /// <summary>
        /// 単位を固定し、ユーザーによる変更を禁止するかどうかを取得または設定します。
        /// </summary>
        public bool IsUnitFixed
        {
            get => (bool)GetValue(IsUnitFixedProperty);
            set => SetValue(IsUnitFixedProperty, value);
        }

        public static readonly DependencyProperty FixedUnitProperty =
            DependencyProperty.Register(nameof(FixedUnit), typeof(string), typeof(CssNumericEditorControl), new PropertyMetadata("px", OnFixedUnitChanged));

        /// <summary>
        /// 固定表示する単位（IsUnitFixedがTrueの時に使用）を取得または設定します。
        /// </summary>
        public string FixedUnit
        {
            get => (string)GetValue(FixedUnitProperty);
            set => SetValue(FixedUnitProperty, value);
        }

        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register(nameof(DefaultValue), typeof(double), typeof(CssNumericEditorControl), new PropertyMetadata(0.0));

        /// <summary>
        /// 未入力状態でスピナーを操作した際の初期値を取得または設定します。
        /// </summary>
        public double DefaultValue
        {
            get => (double)GetValue(DefaultValueProperty);
            set => SetValue(DefaultValueProperty, value);
        }

        public static readonly DependencyProperty DefaultUnitProperty =
            DependencyProperty.Register(nameof(DefaultUnit), typeof(string), typeof(CssNumericEditorControl), new PropertyMetadata("em", OnDefaultUnitChanged));

        /// <summary>
        /// 値が空の場合や、単位が指定されていない場合に使用されるデフォルトの単位を取得または設定します。
        /// </summary>
        public string DefaultUnit
        {
            get => (string)GetValue(DefaultUnitProperty);
            set => SetValue(DefaultUnitProperty, value);
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
            // 初期化
            Loaded += CssNumericEditorControl_Loaded;
        }

        private void CssNumericEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InternalUnit))
            {
                InternalUnit = DefaultUnit;
            }
            UpdateAvailableValues(InternalUnit);
        }
        private static void OnFixedUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CssNumericEditorControl)d;
            if (control.IsUnitFixed)
            {
                control.InternalUnit = e.NewValue as string;
            }
        }

        private static void OnDefaultUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CssNumericEditorControl)d;
            // 値が未設定の場合、初期表示単位を DefaultUnit に合わせる
            if (string.IsNullOrWhiteSpace(control.CssValue) && !control.IsUnitFixed)
            {
                control.InternalUnit = e.NewValue as string;
                control.UpdateAvailableValues(e.NewValue as string);
            }
        }

        private static void OnCssValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CssNumericEditorControl)d;
            if (control._isUpdating) return;

            control._isUpdating = true;
            try
            {
                var (val, unit) = UnitConversionHelper.Split(e.NewValue as string);

                // null の場合は InternalValue を空にする
                control.InternalValue = e.NewValue == null ? "" : val.ToString();

                if (control.IsUnitFixed)
                {
                    control.InternalUnit = control.FixedUnit;
                }
                else
                {
                    // 値が設定された場合はその単位を尊重し、空の場合は DefaultUnit にする
                    control.InternalUnit = string.IsNullOrEmpty(unit) ? control.DefaultUnit : unit;
                }
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
                var currentUnitText = control.IsUnitFixed ? control.FixedUnit : control.InternalUnit;

                // 単位が未設定なら DefaultUnit をデフォルトにする
                if (string.IsNullOrEmpty(currentUnitText) && !control.IsUnitFixed)
                {
                    currentUnitText = control.DefaultUnit;
                    control.InternalUnit = control.DefaultUnit;
                }

                if (e.Property == InternalUnitProperty && !control.IsUnitFixed)
                {
                    control.UpdateAvailableValues(currentUnitText);

                    // 単位が変わった場合は数値を変換
                    if (double.TryParse(currentValText, out var val))
                    {
                        var oldUnit = e.OldValue as string ?? control.DefaultUnit;
                        var newUnit = e.NewValue as string ?? control.DefaultUnit;

                        var supported = new[] { "px", "em", "%" };
                        // 単位なし("") は変換対象外とするか、1.0倍として扱うか。ここでは変換しない。
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
                    // 単位が確定していない場合は DefaultUnit を使う
                    currentUnitText ??= control.DefaultUnit;

                    // マイナス値は 0 に制限
                    if (num < 0) num = 0;

                    // px, % は整数のみに制限
                    if (currentUnitText == "px" || currentUnitText == "%")
                    {
                        num = Math.Round(num);
                        control.InternalValue = num.ToString();
                    }

                    control.CssValue = UnitConversionHelper.Format(num, currentUnitText);
                }
                else
                {
                    // 空文字または不正な入力の場合は null をセットして「未設定」とする
                    control.CssValue = null;
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
            double val;
            if (string.IsNullOrWhiteSpace(InternalValue))
            {
                // 未入力時はデフォルト値から開始
                val = DefaultValue;
            }
            else if (!double.TryParse(InternalValue, out val))
            {
                return;
            }

            var unit = IsUnitFixed ? FixedUnit : InternalUnit;
            var step = (unit == "em" || unit == "rem" || string.IsNullOrEmpty(unit)) ? 0.1 : 1.0;
            var newValue = val + (delta * step);

            // マイナス値は 0 に制限
            if (newValue < 0) newValue = 0;

            // px, % は整数に丸める
            if (InternalUnit == "px" || InternalUnit == "%")
            {
                newValue = Math.Round(newValue);
            }

            InternalValue = UnitConversionHelper.Round(newValue).ToString();
        }

        private void UpdateAvailableValues(string? unit)
        {
            AvailableValues.Clear();
            // unit が null の場合は何もしないが、空文字（単位なし）は許容する
            if (unit == null) return;

            string[] values;
            switch (unit.ToLower())
            {
                case "em":
                case "": // 単位なし（倍率）も em と同じ刻みで提案
                    values = new[] { "0.5", "0.75", "0.8", "0.9", "1.0", "1.1", "1.2", "1.5", "2.0", "3.0" };
                    break;
                case "%":
                    values = new[] { "50", "75", "80", "90", "100", "110", "120", "150", "200", "300" };
                    break;
                case "px":
                default:
                    values = new[] { "9", "10", "11", "12", "14", "16", "18", "20", "24", "28", "32", "36", "48", "64", "72" };
                    break;
            }

            foreach (var v in values)
            {
                AvailableValues.Add(v);
            }
        }
    }
}
