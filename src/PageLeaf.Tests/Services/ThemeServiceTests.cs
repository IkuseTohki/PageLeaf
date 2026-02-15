using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using LeafKit.UI.Services;
using System.Windows;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class ThemeServiceTests
    {
        private Mock<ISettingsService> _mockSettings = null!;
        private Mock<ISystemThemeProvider> _mockSystemTheme = null!;
        private Mock<IThemeManager> _mockThemeManager = null!;
        private ThemeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockSettings = new Mock<ISettingsService>();
            _mockSystemTheme = new Mock<ISystemThemeProvider>();
            _mockThemeManager = new Mock<IThemeManager>();
            _service = new ThemeService(_mockSettings.Object, _mockSystemTheme.Object, _mockThemeManager.Object);
        }

        [TestMethod]
        public void GetActualTheme_ShouldReturnLightTheme_WhenConfigured()
        {
            // Arrange
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Appearance = new AppearanceSettings { Theme = AppTheme.Light } });

            // Act
            var theme = _service.GetActualTheme();

            // Assert
            Assert.AreEqual(AppTheme.Light, theme);
        }

        [TestMethod]
        public void GetActualTheme_ShouldReturnDarkTheme_WhenConfigured()
        {
            // Arrange
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Appearance = new AppearanceSettings { Theme = AppTheme.Dark } });

            // Act
            var theme = _service.GetActualTheme();

            // Assert
            Assert.AreEqual(AppTheme.Dark, theme);
        }

        [TestMethod]
        public void GetActualTheme_ShouldReturnSystemTheme_WhenConfiguredToSystem()
        {
            // Arrange
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Appearance = new AppearanceSettings { Theme = AppTheme.System } });
            _mockSystemTheme.Setup(x => x.GetSystemTheme()).Returns(AppTheme.Dark);

            // Act
            var theme = _service.GetActualTheme();

            // Assert
            Assert.AreEqual(AppTheme.Dark, theme);
            _mockSystemTheme.Verify(x => x.GetSystemTheme(), Times.Once);
        }

        [TestMethod]
        public void ApplyActualTheme_ShouldCallThemeManagerWithCorrectTheme()
        {
            /*
             * テスト観点:
             * ThemeService がテーマを決定し、LeafKit.UI の ThemeManager に対して
             * 適切な型（ライブラリ側の AppTheme）で適用を依頼することを確認する。
             */

            // Arrange: ダークテーマ設定
            _mockSettings.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings { Appearance = new AppearanceSettings { Theme = AppTheme.Dark } });

            // Act
            _service.ApplyActualTheme();

            // Assert: ライブラリの型である LeafKit.UI.Models.AppTheme.Dark が渡されていること
            _mockThemeManager.Verify(x => x.ApplyTheme(
                It.IsAny<Application>(),
                LeafKit.UI.Models.AppTheme.Dark,
                "PageLeaf"),
                Times.Once);
        }
    }
}
