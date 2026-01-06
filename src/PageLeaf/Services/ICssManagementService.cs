using PageLeaf.Models;
using System.Collections.Generic;

namespace PageLeaf.Services
{
    /// <summary>
    /// CSSスタイルのロード、保存、および利用可能なファイルの管理を一元的に行うサービスインターフェースです。
    /// ViewModelと低レベルなCSS/ファイルサービスの間を仲介します。
    /// </summary>
    public interface ICssManagementService
    {
        /// <summary>
        /// アプリケーションで利用可能なCSSファイルのリストを取得します。
        /// </summary>
        /// <returns>CSSファイル名の列挙。ファイルが見つからない場合は空の列挙を返します。</returns>
        IEnumerable<string> GetAvailableCssFileNames();

        /// <summary>
        /// 指定されたCSSファイルを読み込み、スタイル情報オブジェクトとして返します。
        /// </summary>
        /// <param name="cssFileName">読み込むCSSファイル名。</param>
        /// <returns>パースされたスタイル情報。ファイルが存在しない場合は、デフォルト値を持つオブジェクトを返します。</returns>
        /// <exception cref="System.IO.IOException">ファイルの読み込み中にI/Oエラーが発生した場合にスローされます。</exception>
        CssStyleInfo LoadStyle(string cssFileName);

        /// <summary>
        /// 指定されたスタイル情報をCSSファイルに保存します。既存のスタイルを保持しつつ、変更箇所を更新します。
        /// </summary>
        /// <param name="cssFileName">保存先のCSSファイル名。</param>
        /// <param name="styleInfo">保存するスタイル情報。</param>
        /// <exception cref="System.ArgumentNullException">styleInfo が null の場合にスローされます。</exception>
        /// <exception cref="System.IO.IOException">ファイルの書き込み中にI/Oエラーが発生した場合にスローされます。</exception>
        void SaveStyle(string cssFileName, CssStyleInfo styleInfo);

        /// <summary>
        /// 指定されたCSSファイル名に対する物理的なファイルパスを取得します。
        /// </summary>
        /// <param name="cssFileName">対象のCSSファイル名（例: "github.css"）。</param>
        /// <returns>ファイルシステム上の絶対パス。</returns>
        string GetCssPath(string cssFileName);

        /// <summary>
        /// 指定されたCSSファイルの内容を文字列として取得します。
        /// </summary>
        /// <param name="cssFileName">対象のCSSファイル名。</param>
        /// <returns>CSSファイルの内容。</returns>
        string GetCssContent(string cssFileName);

        /// <summary>
        /// 指定されたスタイル情報に基づいて、保存せずにCSS文字列を生成します。
        /// 既存のCSSファイルの内容をベースに更新を行います。
        /// </summary>
        /// <param name="existingCssContent">ベースとなるCSSコンテンツ。</param>
        /// <param name="styleInfo">適用するスタイル情報。</param>
        /// <returns>生成されたCSS文字列。</returns>
        string GenerateCss(string existingCssContent, CssStyleInfo styleInfo);

        /// <summary>
        /// 新しいCSSスタイル（ファイル）を作成します。
        /// </summary>
        /// <param name="styleName">スタイル名（拡張子なしを想定）。</param>
        /// <returns>作成されたファイル名（拡張子付き）。</returns>
        string CreateNewStyle(string styleName);
    }
}
