using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// 指定されたCSSファイルからスタイル情報を読み込むユースケースのインターフェースです。
    /// </summary>
    public interface ILoadCssUseCase
    {
        /// <summary>
        /// CSSファイルの内容と、解析されたスタイル情報を取得します。
        /// </summary>
        /// <param name="cssFileName">読み込むCSSファイルの名前。</param>
        /// <returns>CSSの生テキスト内容と、構造化されたスタイル情報のタプル。</returns>
        (string content, CssStyleInfo styleInfo) Execute(string cssFileName);
    }
}
