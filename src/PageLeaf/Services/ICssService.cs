using System.Collections.Generic;

namespace PageLeaf.Services
{
    /// <summary>
    /// アプリケーションが提供するプリセットCSSファイルの管理を行うサービスインターフェースです。
    /// </summary>
    public interface ICssService
    {
        /// <summary>
        /// 利用可能なCSSファイルのファイル名リストを取得します。
        /// </summary>
        /// <returns>利用可能なCSSファイルのファイル名（拡張子含む）のリスト。</returns>
        IEnumerable<string> GetAvailableCssFileNames();

        /// <param name="cssFileName">パスを取得するCSSファイル名。</param>
        /// <returns>CSSファイルの絶対パス。</returns>
        string GetCssPath(string cssFileName);

        /// <summary>
        /// 新しいCSSファイルをデフォルトの内容で作成します。
        /// </summary>
        /// <param name="fileName">作成するファイル名（拡張子抜き）。</param>
        /// <returns>作成されたファイルの名前（拡張子込み）。</returns>
        string CreateNewCssFile(string fileName);
    }
}
