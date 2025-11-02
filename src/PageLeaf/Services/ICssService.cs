using System.Collections.Generic;

namespace PageLeaf.Services
{
    /// <summary>
    /// CSSファイル関連の操作を定義するインターフェースです。
    /// </summary>
    public interface ICssService
    {
        /// <summary>
        /// 利用可能なCSSファイルのファイル名リストを取得します。
        /// </summary>
        /// <returns>利用可能なCSSファイルのファイル名（拡張子含む）のリスト。</returns>
        IEnumerable<string> GetAvailableCssFileNames();
        string GetCssPath(string cssFileName);
    }
}