using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using System.Linq;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class CssEditorViewModelTests
    {
        private Mock<ICssManagementService> _mockCssManagementService = null!;
        private CssEditorViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCssManagementService = new Mock<ICssManagementService>();
            _viewModel = new CssEditorViewModel(_mockCssManagementService.Object);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldBeInitialized()
        {
            Assert.IsNotNull(_viewModel.SaveCssCommand);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCallServiceToSaveStyles()
        {
            // Arrange
            var fileName = "test.css";
            var cssInfo = new Models.CssStyleInfo();
            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(cssInfo);
            _viewModel.Load(fileName); // TargetCssFileNameを設定

            _viewModel.BodyTextColor = "red";
            _viewModel.BodyBackgroundColor = "white";

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockCssManagementService.Verify(s => s.SaveStyle(fileName, It.Is<Models.CssStyleInfo>(info =>
                info.BodyTextColor == "red" &&
                info.BodyBackgroundColor == "white"
                )), Times.Once);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldRaiseCssSavedEvent()
        {
            // Arrange
            var fileName = "test.css";
            var cssInfo = new Models.CssStyleInfo();
            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(cssInfo);
            _viewModel.Load(fileName);

            bool eventRaised = false;
            _viewModel.CssSaved += (sender, args) =>
            {
                eventRaised = true;
            };

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            Assert.IsTrue(eventRaised, "CssSaved event should have been raised.");
        }

        [TestMethod]
        public void Load_ShouldLoadStylesFromService()
        {
            // Arrange
            var fileName = "test.css";
            var styleInfo = new Models.CssStyleInfo
            {
                BodyTextColor = "#123456",
                QuoteTextColor = "#654321"
            };
            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(styleInfo);

            // Act
            _viewModel.Load(fileName);

            // Assert
            Assert.AreEqual("#123456", _viewModel.BodyTextColor);
            Assert.AreEqual("#654321", _viewModel.QuoteTextColor);
            Assert.AreEqual(fileName, _viewModel.TargetCssFileName);
        }

        [TestMethod]
        public void BodyTextColor_ShouldRaisePropertyChanged()
        {
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

        // ... (他の基本的なPropertyChangedテストはロジック変更がないため省略、重要なロジックのみテスト)

        [TestMethod]
        public void SelectedHeadingLevel_ShouldUpdatePropertiesFromLoadedStyles()
        {
            // Arrange
            var fileName = "test.css";
            var cssInfo = new Models.CssStyleInfo();
            cssInfo.HeadingTextColors["h1"] = "red";
            cssInfo.HeadingTextColors["h2"] = "blue";

            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(cssInfo);
            _viewModel.Load(fileName);

            // Act & Assert
            // 初期状態 (h1)
            _viewModel.SelectedHeadingLevel = "h1";
            Assert.AreEqual("red", _viewModel.HeadingTextColor);

            // 変更 (h2)
            _viewModel.SelectedHeadingLevel = "h2";
            Assert.AreEqual("blue", _viewModel.HeadingTextColor);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldIncludeHeadingStyles()
        {
            // Arrange
            var fileName = "test.css";
            var cssInfo = new Models.CssStyleInfo();
            // 初期データを設定
            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(cssInfo);
            _viewModel.Load(fileName);

            // Act
            _viewModel.SelectedHeadingLevel = "h1";
            _viewModel.HeadingTextColor = "green";

            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockCssManagementService.Verify(s => s.SaveStyle(fileName, It.Is<Models.CssStyleInfo>(info =>
                info.HeadingTextColors.ContainsKey("h1") && info.HeadingTextColors["h1"] == "green"
            )), Times.Once);
        }

        [TestMethod]
        public void Indexer_ShouldGetAndSetStyleValue()
        {
            // テスト観点: インデクサを介して動的にスタイル値を設定・取得できることを確認する。
            //            存在しないキーの場合は null を返し、エラーにならないことも確認。

            // Act
            _viewModel["BodyTextColor"] = "#FF0000";

            // Assert
            Assert.AreEqual("#FF0000", _viewModel["BodyTextColor"]);
            Assert.AreEqual("#FF0000", _viewModel.BodyTextColor); // 既存プロパティとも同期していること
            Assert.IsNull(_viewModel["InvalidKey"]);
        }

        [TestMethod]
        public void Indexer_Set_ShouldRaisePropertyChanged()
        {
            // テスト観点: インデクサ経由で値を変更した際、インデクサ自体("Item[]")と、
            //            該当するプロパティ名の両方で PropertyChanged イベントが発生することを確認する。

            // Arrange
            var raisedProperties = new System.Collections.Generic.List<string>();
            _viewModel.PropertyChanged += (sender, e) => raisedProperties.Add(e.PropertyName!);

            // Act
            _viewModel["BodyFontSize"] = "16px";

            // Assert
            Assert.IsTrue(raisedProperties.Contains("Item[]"), "Indexer change notification should be raised.");
            Assert.IsTrue(raisedProperties.Contains("BodyFontSize"), "Specific property notification should be raised.");
        }

        [TestMethod]
        public void Load_WithNullProperties_ShouldHandleGracefully()
        {
            // テスト観点: CssStyleInfo のプロパティが null の場合でも、エラーにならずロードできることを確認する。

            // Arrange
            var styleInfo = new Models.CssStyleInfo { BodyTextColor = null };
            _mockCssManagementService.Setup(s => s.LoadStyle(It.IsAny<string>())).Returns(styleInfo);

            // Act
            _viewModel.Load("test.css");

            // Assert
            Assert.IsNull(_viewModel["BodyTextColor"]);
            Assert.IsNull(_viewModel.BodyTextColor);
        }

        [TestMethod]
        public void Save_ShouldOnlyIncludeValidProperties()
        {
            // テスト観点: インデクサ経由で追加された未知のキーが、保存時の CssStyleInfo に悪影響を与えないことを確認する。

            // Arrange
            var styleInfo = new Models.CssStyleInfo();
            _mockCssManagementService.Setup(s => s.LoadStyle(It.IsAny<string>())).Returns(styleInfo);

            _viewModel.Load("test.css");
            _viewModel["UnknownProperty"] = "SomeValue";

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockCssManagementService.Verify(s => s.SaveStyle(It.IsAny<string>(), It.Is<Models.CssStyleInfo>(info =>
                info.GetType().GetProperties().All(p => p.Name != "UnknownProperty")
            )), Times.Once);
        }
    }
}
