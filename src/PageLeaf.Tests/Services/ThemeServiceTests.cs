using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Settings;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class ThemeServiceTests
    {
        private Mock<ISettingsService> _mockSettings = null!;
        private Mock<ISystemThemeProvider> _mockSystemTheme = null!;
        private ThemeService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockSettings = new Mock<ISettingsService>();
            _mockSystemTheme = new Mock<ISystemThemeProvider>();
            _service = new ThemeService(_mockSettings.Object, _mockSystemTheme.Object);
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
    }
}
