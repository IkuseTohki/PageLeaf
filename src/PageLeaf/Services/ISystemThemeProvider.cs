using PageLeaf.Models;

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
