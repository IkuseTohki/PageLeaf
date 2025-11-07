using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.ViewModels;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class CssEditorViewModelTests
    {
        [TestMethod]
        public void BodyTextColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: BodyTextColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            var viewModel = new CssEditorViewModel();
            bool wasRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.BodyTextColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            viewModel.BodyTextColor = "#123456";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void BodyBackgroundColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: BodyBackgroundColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            var viewModel = new CssEditorViewModel();
            bool wasRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.BodyBackgroundColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            viewModel.BodyBackgroundColor = "#abcdef";

            // Assert
            Assert.IsTrue(wasRaised);
        }
    }
}
