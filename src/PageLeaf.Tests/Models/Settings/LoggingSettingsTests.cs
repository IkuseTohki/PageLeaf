using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Settings;
using System;

namespace PageLeaf.Tests.Models.Settings
{
    [TestClass]
    public class LoggingSettingsTests
    {
        [TestMethod]
        public void LoggingSettings_DefaultValues_ShouldBeCorrect()
        {
            // テスト観点: ログ設定のデフォルト値が仕様通りであることを確認する。
            var settings = new LoggingSettings();

            Assert.AreEqual(LogOutputLevel.Standard, settings.MinimumLevel);
            Assert.IsTrue(settings.EnableFileLogging);
        }

        [TestMethod]
        public void LoggingSettings_Properties_ShouldBeAssignable()
        {
            // テスト観点: プロパティに値を設定し、正しく保持されることを確認する。
            var settings = new LoggingSettings
            {
                MinimumLevel = LogOutputLevel.Development,
                EnableFileLogging = false
            };

            Assert.AreEqual(LogOutputLevel.Development, settings.MinimumLevel);
            Assert.IsFalse(settings.EnableFileLogging);
        }
    }
}
