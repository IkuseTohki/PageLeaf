using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using Microsoft.Win32;

namespace PageLeaf.Services
{
    public class SystemThemeProvider : ISystemThemeProvider
    {
        public AppTheme GetSystemTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int i && i == 0)
                {
                    return AppTheme.Dark;
                }
            }
            catch { }
            return AppTheme.Light;
        }
    }
}
