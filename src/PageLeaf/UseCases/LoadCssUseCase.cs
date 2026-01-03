using PageLeaf.Models;
using PageLeaf.Services;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// 指定されたCSSファイルからスタイル情報を読み込むユースケースの実装クラスです。
    /// </summary>
    public class LoadCssUseCase : ILoadCssUseCase
    {
        private readonly ICssManagementService _cssManagementService;

        /// <summary>
        /// <see cref="LoadCssUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="cssManagementService">CSS管理サービス。</param>
        public LoadCssUseCase(ICssManagementService cssManagementService)
        {
            _cssManagementService = cssManagementService;
        }

        /// <inheritdoc />
        public (string content, CssStyleInfo styleInfo) Execute(string cssFileName)
        {
            string content = _cssManagementService.GetCssContent(cssFileName);
            CssStyleInfo styleInfo = _cssManagementService.LoadStyle(cssFileName);
            return (content, styleInfo);
        }
    }
}