using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Services;
using PageLeaf.ViewModels;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class CssEditorViewModelTests
    {
        private Mock<IFileService> _mockFileService = null!;
        private Mock<ICssEditorService> _mockCssEditorService = null!;
        private CssEditorViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockFileService = new Mock<IFileService>();
            _mockCssEditorService = new Mock<ICssEditorService>();
            _viewModel = new CssEditorViewModel(_mockFileService.Object, _mockCssEditorService.Object);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldBeInitialized()
        {
            // Arrange
            // Act
            var command = _viewModel.SaveCssCommand;

            // Assert
            Assert.IsNotNull(command);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCallServicesToWriteUpdatedCssToFile()
        {
            // テスト観点: SaveCssCommand実行時に、ファイルの読み込み、更新、書き込みが正しく連携されることを確認する。
            // Arrange
            var filePath = "C:\\temp\\test.css";
            var initialCss = "body { color: black; }";
            var updatedCss = "body { color: red; background-color: white; }";
            
            _viewModel.TargetCssPath = filePath;
            _viewModel.BodyTextColor = "red";
            _viewModel.BodyBackgroundColor = "white";
            _viewModel.HeadingTextColor = "blue";

            _mockFileService.Setup(s => s.ReadAllText(filePath)).Returns(initialCss);
            _mockCssEditorService.Setup(s => s.UpdateCssContent(initialCss, It.Is<Models.CssStyleInfo>(info => 
                info.BodyTextColor == _viewModel.BodyTextColor && 
                info.BodyBackgroundColor == _viewModel.BodyBackgroundColor &&
                info.HeadingTextColor == _viewModel.HeadingTextColor
                )))
                                 .Returns(updatedCss);

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.ReadAllText(filePath), Times.Once);
            _mockCssEditorService.Verify(s => s.UpdateCssContent(initialCss, It.Is<Models.CssStyleInfo>(info =>
                info.BodyTextColor == _viewModel.BodyTextColor &&
                info.BodyBackgroundColor == _viewModel.BodyBackgroundColor &&
                info.HeadingTextColor == _viewModel.HeadingTextColor
                )), Times.Once);
            _mockFileService.Verify(s => s.WriteAllText(filePath, updatedCss), Times.Once);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldRaiseCssSavedEvent()
        {
            // テスト観点: SaveCssCommandの実行が成功したときにCssSavedイベントが発行されることを確認する。
            // Arrange
            var filePath = "C:\\temp\\test.css";
            _viewModel.TargetCssPath = filePath;

            bool eventRaised = false;
            _viewModel.CssSaved += (sender, args) => {
                eventRaised = true;
            };

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            Assert.IsTrue(eventRaised, "CssSaved event should have been raised.");
        }

        [TestMethod]
        public void BodyTextColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: BodyTextColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.BodyTextColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.BodyTextColor = "#123456";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void BodyBackgroundColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: BodyBackgroundColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.BodyBackgroundColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.BodyBackgroundColor = "#abcdef";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void BodyFontSize_ShouldRaisePropertyChangedAndUpdateCss()
        {
            // テスト観点: BodyFontSize プロパティが変更されたときに、PropertyChanged イベントが発火し、
            // ICssEditorService.UpdateCssContent が呼び出され、AppliedCss プロパティが更新されることを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.BodyFontSize))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.BodyFontSize = "20px";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void HeadingTextColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: HeadingTextColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.HeadingTextColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.HeadingTextColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void QuoteTextColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: QuoteTextColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.QuoteTextColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.QuoteTextColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void QuoteBackgroundColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: QuoteBackgroundColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.QuoteBackgroundColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.QuoteBackgroundColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void QuoteBorderColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: QuoteBorderColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.QuoteBorderColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.QuoteBorderColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void TableBorderColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: TableBorderColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.TableBorderColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.TableBorderColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void TableHeaderBackgroundColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: TableHeaderBackgroundColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.TableHeaderBackgroundColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.TableHeaderBackgroundColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void CodeTextColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: CodeTextColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.CodeTextColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.CodeTextColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void CodeBackgroundColor_ShouldRaisePropertyChanged()
        {
            // テスト観点: CodeBackgroundColor プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.CodeBackgroundColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.CodeBackgroundColor = "#ff0000";

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void SelectedHeadingLevel_ShouldUpdateHeadingTextColorAndRaisePropertyChanged()
        {
            // テスト観点: SelectedHeadingLevel プロパティが変更されたときに、
            // HeadingTextColor が更新され、PropertyChanged イベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo();
            cssInfo.HeadingTextColors["h1"] = "rgba(255, 0, 0, 1)"; // Red
            cssInfo.HeadingTextColors["h2"] = "rgba(0, 0, 255, 1)"; // Blue

            // LoadStylesが呼ばれたときに内部状態が設定されるようにモックを設定
            _viewModel.LoadStyles(cssInfo);

            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.HeadingTextColor))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.SelectedHeadingLevel = "h2";

            // Assert
            Assert.IsTrue(wasRaised, "PropertyChanged for HeadingTextColor should have been raised.");
            Assert.AreEqual("rgba(0, 0, 255, 1)", _viewModel.HeadingTextColor);
        }
    }
}
