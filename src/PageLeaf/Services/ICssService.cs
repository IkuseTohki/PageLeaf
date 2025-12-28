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

        /// <summary>
        /// 指定されたCSSファイル名の物理パスを取得します。
        /// </summary>
        /// <param name="cssFileName">対象のCSSファイル名。</param>
        /// <returns>CSSファイルへのフルパス。</returns>
        string GetCssPath(string cssFileName);
    }
}
