
using PageLeaf.Models;

namespace PageLeaf.Services
{
    public interface ISettingsService
    {
        ApplicationSettings CurrentSettings { get; }
        ApplicationSettings LoadSettings();
        void SaveSettings(ApplicationSettings settings);
    }
}
