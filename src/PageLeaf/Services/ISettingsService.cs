
using PageLeaf.Models;

namespace PageLeaf.Services
{
    public interface ISettingsService
    {
        ApplicationSettings LoadSettings();
        void SaveSettings(ApplicationSettings settings);
    }
}
