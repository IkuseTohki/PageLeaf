using PageLeaf.Models;
using PageLeaf.Models.Settings;
using LeafKit.UI.Services;
using System.Windows;

namespace PageLeaf.Services
{
    public interface IThemeService
    {
        AppTheme GetActualTheme();
        void ApplyActualTheme();
    }

    public class ThemeService : IThemeService
    {
        private readonly ISettingsService _settingsService;
        private readonly ISystemThemeProvider _systemThemeProvider;
        private readonly IThemeManager _themeManager;

        public ThemeService(ISettingsService settingsService, ISystemThemeProvider systemThemeProvider, IThemeManager themeManager)
        {
            _settingsService = settingsService;
            _systemThemeProvider = systemThemeProvider;
            _themeManager = themeManager;
        }

        public AppTheme GetActualTheme()
        {
            var setting = _settingsService.CurrentSettings.Appearance.Theme;
            if (setting == AppTheme.System)
            {
                return _systemThemeProvider.GetSystemTheme();
            }
            return setting;
        }

        public void ApplyActualTheme()
        {
            var actualTheme = GetActualTheme();
            var libraryTheme = actualTheme == AppTheme.Dark
                ? LeafKit.UI.Models.AppTheme.Dark
                : LeafKit.UI.Models.AppTheme.Light;

            _themeManager.ApplyTheme(Application.Current, libraryTheme, "PageLeaf");
        }
    }
}
