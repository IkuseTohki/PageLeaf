using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class ThemeServiceTests
    {
        private Mock<ISettingsService> _mockSettings = null!;
        private Mock<ISystemThemeProvider> _mockSystemTheme = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockSettings = new Mock<ISettingsService>();
            _mockSystemTheme = new Mock<ISystemThemeProvider>();
        }

        [TestMethod]
        public void GetActualTheme_ShouldReturnLight_WhenSettingsIsLight()
        {
            // テスト観点: 設定がライトモードなら、OS設定に関わらずライトモードを返す。
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Theme = AppTheme.Light });
            _mockSystemTheme.Setup(x => x.GetSystemTheme()).Returns(AppTheme.Dark);

            var service = new ThemeService(_mockSettings.Object, _mockSystemTheme.Object);
            Assert.AreEqual(AppTheme.Light, service.GetActualTheme());
        }

        [TestMethod]
        public void GetActualTheme_ShouldReturnDark_WhenSettingsIsDark()
        {
            // テスト観点: 設定がダークモードなら、OS設定に関わらずダークモードを返す。
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Theme = AppTheme.Dark });
            _mockSystemTheme.Setup(x => x.GetSystemTheme()).Returns(AppTheme.Light);

            var service = new ThemeService(_mockSettings.Object, _mockSystemTheme.Object);
            Assert.AreEqual(AppTheme.Dark, service.GetActualTheme());
        }

        [TestMethod]
        public void GetActualTheme_ShouldFollowSystem_WhenSettingsIsSystem()
        {
            // テスト観点: 設定がシステム依存なら、OS設定に従う。
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Theme = AppTheme.System });

            _mockSystemTheme.Setup(x => x.GetSystemTheme()).Returns(AppTheme.Dark);
            var service = new ThemeService(_mockSettings.Object, _mockSystemTheme.Object);
            Assert.AreEqual(AppTheme.Dark, service.GetActualTheme());

            _mockSystemTheme.Setup(x => x.GetSystemTheme()).Returns(AppTheme.Light);
            Assert.AreEqual(AppTheme.Light, service.GetActualTheme());
        }
    }
}
