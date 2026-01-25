using PageLeaf.Models;

namespace PageLeaf.Services
{
    public interface IThemeService
    {
        AppTheme GetActualTheme();
    }

    public class ThemeService : IThemeService
    {
        private readonly ISettingsService _settingsService;
        private readonly ISystemThemeProvider _systemThemeProvider;

        public ThemeService(ISettingsService settingsService, ISystemThemeProvider systemThemeProvider)
        {
            _settingsService = settingsService;
            _systemThemeProvider = systemThemeProvider;
        }

        public AppTheme GetActualTheme()
        {
            var setting = _settingsService.CurrentSettings.Theme;
            if (setting == AppTheme.System)
            {
                return _systemThemeProvider.GetSystemTheme();
            }
            return setting;
        }
    }
}
