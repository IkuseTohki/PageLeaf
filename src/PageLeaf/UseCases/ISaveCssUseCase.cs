using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// スタイル情報を指定されたCSSファイルに保存するユースケースのインターフェースです。
    /// </summary>
    public interface ISaveCssUseCase
    {
        /// <summary>
        /// スタイル情報をCSSファイルとして書き出します。
        /// </summary>
        /// <param name="cssFileName">保存先のCSSファイル名。</param>
        /// <param name="styleInfo">保存するスタイル情報。</param>
        void Execute(string cssFileName, CssStyleInfo styleInfo);
    }
}
