using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

namespace PageLeaf.Services
{
    /// <summary>
    /// システム（OS）のテーマ設定を取得するインターフェース。
    /// </summary>
    public interface ISystemThemeProvider
    {
        AppTheme GetSystemTheme();
    }
}
