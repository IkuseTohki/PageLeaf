using System;
using System.Collections.Generic;
using PageLeaf.Models;

namespace PageLeaf.Models.Settings
{
    /// <summary>
    /// 設定に保存するための追加フロントマタープロパティを表します。
    /// </summary>
    public class FrontMatterAdditionalProperty
    {
        /// <summary>
        /// プロパティのキー。
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// プロパティの値。
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// アプリケーションの設定を保持するモデルクラスです。
    /// </summary>
    public class ApplicationSettings
    {
        private int _indentSize = 4;
        private double _editorFontSize = 14.0;

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
        /// インデントに使用するスペースの数 (1-32)。
        /// </summary>
        public int IndentSize
        {
            get => _indentSize;
            set
            {
                if (value < 1 || value > 32)
                    throw new ArgumentOutOfRangeException(nameof(IndentSize), "Indent size must be between 1 and 32.");
                _indentSize = value;
            }
        }

        /// <summary>
        /// タブの代わりにスペースを使用してインデントするかどうか。
        /// </summary>
        public bool UseSpacesForIndent { get; set; } = true;

        /// <summary>
        /// エディタのフォントサイズ (1-100)。
        /// </summary>
        public double EditorFontSize
        {
            get => _editorFontSize;
            set
            {
                if (value < 1.0 || value > 100.0)
                    throw new ArgumentOutOfRangeException(nameof(EditorFontSize), "Font size must be between 1 and 100.");
                _editorFontSize = value;
            }
        }

        /// <summary>
        /// プレビューの最上部にフロントマターのタイトルを表示するかどうか。
        /// </summary>
        public bool ShowTitleInPreview { get; set; } = true;

        /// <summary>
        /// 新規作成時にフロントマターを自動挿入するかどうか。
        /// </summary>
        public bool AutoInsertFrontMatter { get; set; } = true;

        /// <summary>
        /// 自動挿入される追加のフロントマタープロパティ。
        /// </summary>
        public List<FrontMatterAdditionalProperty> AdditionalFrontMatter { get; set; } = new List<FrontMatterAdditionalProperty>();

        /// <summary>
        /// ライブラリ（Highlight.js, Mermaid）の読み込み先。
        /// </summary>
        public ResourceSource LibraryResourceSource { get; set; } = ResourceSource.Local;

        /// <summary>
        /// アプリケーションの表示テーマ。
        /// </summary>
        public AppTheme Theme { get; set; } = AppTheme.System;

        /// <summary>
        /// 保存時に脚注番号を振り直すかどうか。
        /// </summary>
        public bool RenumberFootnotesOnSave { get; set; } = true;

        /// <summary>
        /// 現在の設定に基づいたインデント文字列を取得します。
        /// </summary>
        /// <returns>スペース文字列またはタブ文字列。</returns>
        public string GetIndentString()
        {
            return UseSpacesForIndent ? new string(' ', IndentSize) : "\t";
        }

        /// <summary>
        /// 行頭のインデントを1レベル分削除します。
        /// </summary>
        /// <param name="line">対象の行。</param>
        /// <returns>インデント削除後の行。</returns>
        public string DecreaseIndent(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;

            // タブで始まる場合
            if (line.StartsWith("\t"))
            {
                return line.Substring(1);
            }

            // スペースで始まる場合、設定されたインデント幅分削除を試みる
            int spaceCount = 0;
            while (spaceCount < IndentSize && spaceCount < line.Length && line[spaceCount] == ' ')
            {
                spaceCount++;
            }

            if (spaceCount > 0)
            {
                return line.Substring(spaceCount);
            }

            return line;
        }

        /// <summary>
        /// 行頭に1レベル分のインデントを追加します。
        /// </summary>
        /// <param name="line">対象の行。</param>
        /// <returns>インデント追加後の行。</returns>
        public string IncreaseIndent(string line)
        {
            return GetIndentString() + line;
        }

        /// <summary>
        /// デフォルトコンストラクタ。
        /// </summary>
        public ApplicationSettings() { }
    }
}
