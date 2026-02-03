using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

namespace PageLeaf.Services
{
    /// <summary>
    /// CSSテキストの解析と、スタイル情報に基づいた更新を行うサービスインターフェースです。
    /// </summary>
    public interface ICssEditorService
    {
        /// <summary>
        /// CSSコンテンツを解析し、構造化されたスタイル情報に変換します。
        /// </summary>
        /// <param name="cssContent">解析対象のCSS文字列。</param>
        /// <returns>パースされたスタイル情報。無効な形式の場合はデフォルト値を返します。</returns>
        /// <exception cref="System.ArgumentNullException">cssContent が null の場合にスローされます。</exception>
        CssStyleInfo ParseCss(string cssContent);

        /// <summary>
        /// CSSコンテンツを解析し、ドメインモデル CssStyleProfile に変換します。
        /// </summary>
        /// <param name="cssContent">解析対象のCSS文字列。</param>
        /// <returns>パースされたスタイルプロファイル。</returns>
        CssStyleProfile ParseToProfile(string cssContent);

        /// <summary>
        /// 既存のCSSコンテンツに対し、指定されたスタイル情報を反映した新しいCSS文字列を生成します。
        /// 既存の不明なスタイル設定は保持されます。
        /// </summary>
        /// <param name="existingCss">元のCSS文字列。</param>
        /// <param name="styleInfo">反映する新しいスタイル情報。</param>
        /// <returns>更新後のCSS文字列。</returns>
        /// <exception cref="System.ArgumentNullException">styleInfo が null の場合にスローされます。</exception>
        string UpdateCssContent(string existingCss, CssStyleInfo styleInfo);

        /// <summary>
        /// 既存のCSSコンテンツに対し、スタイルプロファイルを反映した新しいCSS文字列を生成します。
        /// </summary>
        /// <param name="existingCss">元のCSS文字列。</param>
        /// <param name="profile">反映するスタイルプロファイル。</param>
        /// <returns>更新後のCSS文字列。</returns>
        string UpdateCssFromProfile(string existingCss, CssStyleProfile profile);
    }
}
