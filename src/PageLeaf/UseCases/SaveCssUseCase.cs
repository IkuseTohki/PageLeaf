using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// スタイル情報をCSSファイルに保存するユースケースの実装クラスです。
    /// </summary>
    public class SaveCssUseCase : ISaveCssUseCase
    {
        private readonly ICssManagementService _cssManagementService;

        /// <summary>
        /// <see cref="SaveCssUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="cssManagementService">CSS管理サービス。</param>
        public SaveCssUseCase(ICssManagementService cssManagementService)
        {
            _cssManagementService = cssManagementService;
        }

        /// <inheritdoc />
        public void Execute(string cssFileName, CssStyleInfo styleInfo)
        {
            _cssManagementService.SaveStyle(cssFileName, styleInfo);
        }
    }
}
