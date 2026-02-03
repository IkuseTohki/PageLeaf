namespace PageLeaf.Models.Css
{
    /// <summary>
    /// CSSプロパティのデフォルト値（推奨値）およびデフォルト単位を定義します。
    /// 未設定状態（null）から値を設定する際の初期値として使用されます。
    /// </summary>
    public class CssDefaults
    {
        public static CssDefaults Instance { get; } = new CssDefaults();

        public TitleDefaults Title { get; } = new TitleDefaults();
        public GeneralDefaults General { get; } = new GeneralDefaults();
        public HeadingsDefaults Headings { get; } = new HeadingsDefaults();
        public ListDefaults List { get; } = new ListDefaults();
        public TableDefaults Table { get; } = new TableDefaults();
        public QuoteDefaults Quote { get; } = new QuoteDefaults();
        public FootnoteDefaults Footnote { get; } = new FootnoteDefaults();

        public class TitleDefaults
        {
            public double FontSize => 2.0;
            public string FontSizeUnit => "em";
            public double MarginBottom => 1.2;
            public string MarginBottomUnit => "em";
        }

        public class GeneralDefaults
        {
            public double FontSize => 1.0;
            public string FontSizeUnit => "em";

            // 行間は単位なし（倍率）を推奨
            public double LineHeight => 1.6;
            public string LineHeightUnit => "";

            public double MarginBottom => 1.0;
            public string MarginBottomUnit => "em";

            public double TextIndent => 1.0;
            public string TextIndentUnit => "em";
        }

        public class HeadingsDefaults
        {
            public double FontSize => 1.5;
            public string FontSizeUnit => "em";
        }

        public class ListDefaults
        {
            public double MarkerSize => 1.0;
            public string MarkerSizeUnit => "em";

            public double Indent => 2.0;
            public string IndentUnit => "em";
        }

        public class TableDefaults
        {
            public double BorderWidth => 1.0;
            public string BorderWidthUnit => "px"; // Fixed

            public double FontSize => 1.1;
            public string FontSizeUnit => "em";

            public double CellPadding => 8.0;
            public string CellPaddingUnit => "px"; // Fixed
        }

        public class QuoteDefaults
        {
            public double BorderWidth => 4.0;
            public string BorderWidthUnit => "px"; // Fixed
        }

        public class FootnoteDefaults
        {
            public double FontSize => 0.85;
            public string FontSizeUnit => "em";

            public double MarginTop => 2.0;
            public string MarginTopUnit => "em";

            public double BorderWidth => 1.0;
            public string BorderWidthUnit => "px"; // Fixed

            public double LineHeight => 1.5;
            public string LineHeightUnit => ""; // Unitless
        }
    }
}
