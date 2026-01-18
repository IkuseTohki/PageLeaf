using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Services;
using PageLeaf.UseCases;
using PageLeaf.ViewModels;
using System.Linq;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class CssEditorViewModelTests
    {
        private Mock<ICssManagementService> _mockCssManagementService = null!;
        private Mock<ILoadCssUseCase> _mockLoadCssUseCase = null!;
        private Mock<ISaveCssUseCase> _mockSaveCssUseCase = null!;
        private Mock<IDialogService> _mockDialogService = null!;
        private Mock<ISettingsService> _mockSettingsService = null!;
        private CssEditorViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCssManagementService = new Mock<ICssManagementService>();
            _mockLoadCssUseCase = new Mock<ILoadCssUseCase>();
            _mockSaveCssUseCase = new Mock<ISaveCssUseCase>();
            _mockDialogService = new Mock<IDialogService>();
            _mockSettingsService = new Mock<ISettingsService>();

            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(new PageLeaf.Models.ApplicationSettings());

            _mockCssManagementService.Setup(s => s.GetCssContent(It.IsAny<string>())).Returns("");
            _mockCssManagementService.Setup(s => s.GenerateCss(It.IsAny<string>(), It.IsAny<PageLeaf.Models.CssStyleInfo>())).Returns("");
            _mockLoadCssUseCase.Setup(u => u.Execute(It.IsAny<string>())).Returns(("", new PageLeaf.Models.CssStyleInfo()));

            _viewModel = new CssEditorViewModel(
                _mockCssManagementService.Object,
                _mockLoadCssUseCase.Object,
                _mockSaveCssUseCase.Object,
                _mockDialogService.Object,
                _mockSettingsService.Object);
        }

        [TestMethod]
        public void CodeStyleProperties_ShouldBeAccessible()
        {
            // テスト観点: 新しく追加されたインライン・ブロックコード用のプロパティが正しく動作することを確認する。
            _viewModel.InlineCodeTextColor = "#111111";
            _viewModel.InlineCodeBackgroundColor = "#222222";
            _viewModel.BlockCodeTextColor = "#333333";
            _viewModel.BlockCodeBackgroundColor = "#444444";

            Assert.AreEqual("#111111", _viewModel.InlineCodeTextColor);
            Assert.AreEqual("#222222", _viewModel.InlineCodeBackgroundColor);
            Assert.AreEqual("#333333", _viewModel.BlockCodeTextColor);
            Assert.AreEqual("#444444", _viewModel.BlockCodeBackgroundColor);
            Assert.IsTrue(_viewModel.IsDirty);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldBeInitialized()
        {
            Assert.IsNotNull(_viewModel.SaveCssCommand);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCallUseCaseToSaveStyles()
        {
            // Arrange
            var fileName = "test.css";
            var cssInfo = new PageLeaf.Models.CssStyleInfo();
            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("", cssInfo));
            _viewModel.Load(fileName); // TargetCssFileNameを設定

            _viewModel.BodyTextColor = "red";
            _viewModel.BodyBackgroundColor = "white";

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockSaveCssUseCase.Verify(u => u.Execute(fileName, It.Is<PageLeaf.Models.CssStyleInfo>(info =>
                info.BodyTextColor == "red" &&
                info.BodyBackgroundColor == "white"
                )), Times.Once);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldRaiseCssSavedEvent()
        {
            // Arrange
            var fileName = "test.css";
            var cssInfo = new PageLeaf.Models.CssStyleInfo();
            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("", cssInfo));
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
        public void Load_ShouldLoadStylesFromUseCase()
        {
            // Arrange
            var fileName = "test.css";
            var styleInfo = new PageLeaf.Models.CssStyleInfo
            {
                BodyTextColor = "#123456",
                QuoteTextColor = "#654321"
            };
            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("some content", styleInfo));

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
            var cssInfo = new PageLeaf.Models.CssStyleInfo();
            cssInfo.HeadingTextColors["h1"] = "red";
            cssInfo.HeadingTextColors["h2"] = "blue";

            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("", cssInfo));
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
            var cssInfo = new PageLeaf.Models.CssStyleInfo();
            // 初期データを設定
            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("", cssInfo));
            _viewModel.Load(fileName);

            // Act
            _viewModel.SelectedHeadingLevel = "h1";
            _viewModel.HeadingTextColor = "green";

            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockSaveCssUseCase.Verify(u => u.Execute(fileName, It.Is<PageLeaf.Models.CssStyleInfo>(info =>
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
            var styleInfo = new PageLeaf.Models.CssStyleInfo { BodyTextColor = null };
            _mockLoadCssUseCase.Setup(u => u.Execute(It.IsAny<string>())).Returns(("", styleInfo));

            // Act
            _viewModel.Load("test.css");

            // Assert
            Assert.IsNull(_viewModel["BodyTextColor"]);
            Assert.IsNull(_viewModel.BodyTextColor);
        }

        [TestMethod]
        public void Load_WhenFontSizeIsMissing_ShouldBeNull()
        {
            // テスト観点: CSSファイルにフォントサイズ指定がない場合、
            //            デフォルト値を強制せず null のまま保持されることを確認する。
            //            （ブラウザのデフォルトスタイルを上書きしないため）

            // Arrange
            var styleInfo = new PageLeaf.Models.CssStyleInfo(); // 全プロパティが null
            _mockLoadCssUseCase.Setup(u => u.Execute(It.IsAny<string>())).Returns(("", styleInfo));

            // Act
            _viewModel.Load("empty.css");

            // Assert
            Assert.IsNull(_viewModel.BodyFontSize, "Body should be null if not set in CSS");
            _viewModel.SelectedHeadingLevel = "h1";
            Assert.IsNull(_viewModel.HeadingFontSize, "h1 should be null if not set in CSS");
        }

        [TestMethod]
        public void ResetCommand_ShouldReloadStylesFromUseCase()
        {
            // テスト観点: ResetCommand を実行した際、TargetCssFileName を使用して
            //            UseCase からスタイルが再読み込みされることを確認する。

            // Arrange
            var fileName = "reset_test.css";
            var initialStyle = new PageLeaf.Models.CssStyleInfo { BodyTextColor = "black" };
            var updatedStyle = new PageLeaf.Models.CssStyleInfo { BodyTextColor = "white" };

            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("", initialStyle));
            _viewModel.Load(fileName);

            // 編集して値を汚す
            _viewModel.BodyTextColor = "red";
            Assert.AreEqual("red", _viewModel.BodyTextColor);

            // リセット後の期待値をセットアップ
            _mockLoadCssUseCase.Setup(u => u.Execute(fileName)).Returns(("", updatedStyle));

            // Act
            _viewModel.ResetCommand.Execute(null);

            // Assert
            Assert.AreEqual("white", _viewModel.BodyTextColor, "Style should be reloaded from UseCase after reset");
            _mockLoadCssUseCase.Verify(u => u.Execute(fileName), Times.Exactly(2));
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
            _mockLoadCssUseCase.Setup(u => u.Execute("test.css")).Returns(("", new PageLeaf.Models.CssStyleInfo()));
            _viewModel.Load("test.css");
            _viewModel.BodyTextColor = "NewColor";

            _viewModel.SaveCssCommand.Execute(null);

            Assert.IsFalse(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeFalse_AfterReset()
        {
            _mockLoadCssUseCase.Setup(u => u.Execute("test.css")).Returns(("", new PageLeaf.Models.CssStyleInfo()));
            _viewModel.Load("test.css");
            _viewModel.BodyTextColor = "NewColor";

            _viewModel.ResetCommand.Execute(null);

            Assert.IsFalse(_viewModel.IsDirty);
        }

        [TestMethod]
        public void IsDirty_ShouldBeFalse_AfterLoad()
        {
            _mockLoadCssUseCase.Setup(u => u.Execute("another.css")).Returns(("", new PageLeaf.Models.CssStyleInfo()));
            _viewModel.BodyTextColor = "NewColor";
            _viewModel.Load("another.css");
            Assert.IsFalse(_viewModel.IsDirty);
        }

        [TestMethod]
        public void SelectColorCommand_ShouldUpdateColor_WhenDialogReturnsColor()
        {
            // Arrange
            var propertyName = "BodyTextColor";
            var initialColor = "#000000";
            var selectedColor = "#FFFFFF";
            _viewModel.BodyTextColor = initialColor;
            _mockDialogService.Setup(s => s.ShowColorPickerDialog(initialColor)).Returns(selectedColor);

            // Act
            _viewModel.SelectColorCommand.Execute(propertyName);

            // Assert
            Assert.AreEqual(selectedColor, _viewModel.BodyTextColor);
            Assert.IsTrue(_viewModel.IsDirty);
        }

        [TestMethod]
        public void SelectColorCommand_ShouldNotUpdateColor_WhenDialogIsCancelled()
        {
            // Arrange
            var propertyName = "BodyTextColor";
            var initialColor = "#000000";
            _viewModel.BodyTextColor = initialColor;
            _mockDialogService.Setup(s => s.ShowColorPickerDialog(initialColor)).Returns((string?)null);

            // Act
            _viewModel.SelectColorCommand.Execute(propertyName);

            // Assert
            Assert.AreEqual(initialColor, _viewModel.BodyTextColor);
        }

        [TestMethod]
        public void SelectColorCommand_ShouldHandleHeadingTextColorMapping()
        {
            // Arrange
            _viewModel.SelectedHeadingLevel = "h2";
            var initialColor = "#111111";
            var selectedColor = "#222222";
            _viewModel.HeadingTextColor = initialColor;
            _mockDialogService.Setup(s => s.ShowColorPickerDialog(initialColor)).Returns(selectedColor);

            // Act
            _viewModel.SelectColorCommand.Execute("HeadingTextColor");

            // Assert
            Assert.AreEqual(selectedColor, _viewModel.HeadingTextColor);
            Assert.AreEqual(selectedColor, _viewModel["h2.TextColor"]);
        }

        [TestMethod]
        public void TableBorderStyle_SetNull_ShouldBecomeSolid()
        {
            // テスト観点: TableBorderStyle に null または空文字をセットした際、
            //            ガードロジックにより自動的に "solid" になることを確認する。

            // Act
            _viewModel.TableBorderStyle = null;

            // Assert
            Assert.AreEqual("solid", _viewModel.TableBorderStyle);
        }

        [TestMethod]
        public void QuoteBorderStyle_SetNull_ShouldBecomeSolid()
        {
            // テスト観点: QuoteBorderStyle に null または空文字をセットした際、
            //            ガードロジックにより自動的に "solid" になることを確認する。

            // Act
            _viewModel.QuoteBorderStyle = "";

            // Assert
            Assert.AreEqual("solid", _viewModel.QuoteBorderStyle);
        }

        [TestMethod]
        public void IsTitleTabVisible_ShouldReflectSettings()
        {
            // テスト観点: IsTitleTabVisible プロパティが設定サービスの ShowTitleInPreview を正しく反映することを確認する。

            // Arrange (Setting = true)
            var settings = new PageLeaf.Models.ApplicationSettings { ShowTitleInPreview = true };
            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(settings);
            _viewModel.NotifySettingsChanged();

            // Assert
            Assert.IsTrue(_viewModel.IsTitleTabVisible);

            // Arrange (Setting = false)
            settings.ShowTitleInPreview = false;
            _viewModel.NotifySettingsChanged();

            // Assert
            Assert.IsFalse(_viewModel.IsTitleTabVisible);
        }

        [TestMethod]
        public void SelectedTab_ShouldSwitch_WhenTitleTabBecomesHidden()
        {
            // テスト観点: タイトルタブが非表示になった際、もしタイトルタブが選択されていたら別のタブへ切り替わることを確認する。

            // Arrange
            var settings = new PageLeaf.Models.ApplicationSettings { ShowTitleInPreview = true };
            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(settings);
            _viewModel.NotifySettingsChanged();
            _viewModel.SelectedTab = CssEditorTab.Title;

            // Act: 非表示にする
            settings.ShowTitleInPreview = false;
            _viewModel.NotifySettingsChanged();

            // Assert
            Assert.AreNotEqual(CssEditorTab.Title, _viewModel.SelectedTab, "SelectedTab should have switched away from Title.");
            Assert.AreEqual(CssEditorTab.General, _viewModel.SelectedTab, "SelectedTab should default to General.");
        }
    }
}
