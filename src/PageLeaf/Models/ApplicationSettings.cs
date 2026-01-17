
namespace PageLeaf.Models
{
    public class ApplicationSettings
    {
        /// <summary>
        /// 現在選択されているCSSファイル名。
        /// </summary>
        public string SelectedCss { get; set; } = "";

        /// <summary>
        /// コードブロックのハイライトに使用するテーマ名（例: "github.css"）。
        /// </summary>
        public string CodeBlockTheme { get; set; } = "github.css";

        /// <summary>
        /// CSSエディタの設定でコードブロックのスタイルを上書きするかどうか。
        /// </summary>
        public bool UseCustomCodeBlockStyle { get; set; } = false;

        /// <summary>
        /// 画像を保存するディレクトリ（Markdownファイルからの相対パス）。
        /// </summary>
        public string ImageSaveDirectory { get; set; } = "images";

        /// <summary>
        /// 画像のファイル名テンプレート。
        /// {Date}, {Time}, {FileName} 等の変数が使用可能。
        /// </summary>
        public string ImageFileNameTemplate { get; set; } = "image_{Date}_{Time}";

        /// <summary>
        /// インデントに使用するスペースの数。
        /// </summary>
        public int IndentSize { get; set; } = 4;

        /// <summary>
        /// タブの代わりにスペースを使用してインデントするかどうか。
        /// </summary>
        public bool UseSpacesForIndent { get; set; } = true;

        /// <summary>
        /// 新規作成時にフロントマターを自動挿入するかどうか。
        /// </summary>
        public bool AutoInsertFrontMatter { get; set; } = true;

        // Default constructor for deserialization
        public ApplicationSettings() { }
    }
}
