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
            _viewModel.BodyTextColor = "#abcdef"; // Make it dirty
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
        public void GlobalUnit_Change_ShouldConvertValues()
        {
            // テスト観点: 全体の単位を px から em に変更した際、
            //            既存のフォントサイズ数値が適切に変換されることを確認する。

            // Arrange
            _viewModel.GlobalUnit = "px";
            _viewModel.BodyFontSize = "16"; // 16px
            _viewModel.SelectedHeadingLevel = "h1";
            _viewModel.HeadingFontSize = "32"; // 32px

            // Act: px -> em に変更
            _viewModel.GlobalUnit = "em";

            // Assert
            Assert.AreEqual("1", _viewModel.BodyFontSize, "16px should be converted to 1em");
            Assert.AreEqual("2", _viewModel.HeadingFontSize, "32px should be converted to 2em");
        }

        [TestMethod]
        public void Load_WhenFontSizeIsMissing_ShouldUseDefaultValue()
        {
            // テスト観点: CSSファイルにフォントサイズ指定がない場合、
            //            定義された標準的なデフォルト値(16pxベース)が適用されることを確認する。

            // Arrange
            var styleInfo = new Models.CssStyleInfo(); // 全プロパティが null
            _mockCssManagementService.Setup(s => s.LoadStyle(It.IsAny<string>())).Returns(styleInfo);
            _viewModel.GlobalUnit = "px";

            // Act
            _viewModel.Load("empty.css");

            // Assert
            Assert.AreEqual("16", _viewModel.BodyFontSize, "Body default should be 16px");
            _viewModel.SelectedHeadingLevel = "h1";
            Assert.AreEqual("32", _viewModel.HeadingFontSize, "h1 default should be 32px");
        }

        [TestMethod]
        public void GlobalUnit_Change_ToPercent_ShouldConvertValues()
        {
            // テスト観点: 単位を em から % に変更した際、適切に変換されることを確認する。

            // Arrange
            _viewModel.GlobalUnit = "em";
            _viewModel.BodyFontSize = "1.5"; // 1.5em

            // Act: em -> % に変更
            _viewModel.GlobalUnit = "%";

            // Assert
            Assert.AreEqual("150", _viewModel.BodyFontSize, "1.5em should be converted to 150%");
        }
        [TestMethod]
        public void ResetCommand_ShouldReloadStylesFromService()
        {
            // テスト観点: ResetCommand を実行した際、TargetCssFileName を使用して
            //            サービスからスタイルが再読み込みされることを確認する。

            // Arrange
            var fileName = "reset_test.css";
            var initialStyle = new Models.CssStyleInfo { BodyTextColor = "black" };
            var updatedStyle = new Models.CssStyleInfo { BodyTextColor = "white" };

            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(initialStyle);
            _viewModel.Load(fileName);

            // 編集して値を汚す
            _viewModel.BodyTextColor = "red";
            Assert.AreEqual("red", _viewModel.BodyTextColor);

            // リセット後の期待値をセットアップ
            _mockCssManagementService.Setup(s => s.LoadStyle(fileName)).Returns(updatedStyle);

            // Act
            _viewModel.ResetCommand.Execute(null);

            // Assert
            Assert.AreEqual("white", _viewModel.BodyTextColor, "Style should be reloaded from service after reset");
            _mockCssManagementService.Verify(s => s.LoadStyle(fileName), Times.Exactly(2));
        }

        [TestMethod]
        public void IsDirty_ShouldBeFalse_AfterInitialization()
        {
            Assert.IsFalse(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeTrue_AfterPropertyChange()
        {
            _viewModel.BodyTextColor = "NewColor";
            Assert.IsTrue(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeTrue_AfterHeadingPropertyChange()
        {
            _viewModel.SelectedHeadingLevel = "h1";
            _viewModel.HeadingTextColor = "NewColor";
            Assert.IsTrue(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeFalse_AfterSave()
        {
            _mockCssManagementService.Setup(s => s.LoadStyle("test.css")).Returns(new Models.CssStyleInfo());
            _viewModel.Load("test.css");
            _viewModel.BodyTextColor = "NewColor";

            _viewModel.SaveCssCommand.Execute(null);

            Assert.IsFalse(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeFalse_AfterReset()
        {
            _mockCssManagementService.Setup(s => s.LoadStyle("test.css")).Returns(new Models.CssStyleInfo());
            _viewModel.Load("test.css");
            _viewModel.BodyTextColor = "NewColor";

            _viewModel.ResetCommand.Execute(null);

            Assert.IsFalse(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeFalse_AfterLoad()
        {
            _mockCssManagementService.Setup(s => s.LoadStyle("another.css")).Returns(new Models.CssStyleInfo());
            _viewModel.BodyTextColor = "NewColor";
            _viewModel.Load("another.css");
            Assert.IsFalse(_viewModel.IsDirty);
        }
    }
}
