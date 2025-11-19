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
            _viewModel.HeadingTextColor = "blue"; // これはSelectedHeadingLevelに依存するため、直接検証はしない

            // 見出しスタイルをセット
            var initialCssInfo = new Models.CssStyleInfo();
            initialCssInfo.HeadingTextColors["h1"] = "rgba(255, 0, 0, 1)";
            initialCssInfo.HeadingFontSizes["h1"] = "24px";
            initialCssInfo.HeadingFontFamilies["h1"] = "Arial";
            initialCssInfo.HeadingStyleFlags["h1"] = new Models.HeadingStyleFlags { IsBold = true };
            initialCssInfo.HeadingTextColors["h2"] = "rgba(0, 0, 255, 1)";
            initialCssInfo.HeadingFontSizes["h2"] = "20px";
            initialCssInfo.HeadingFontFamilies["h2"] = "Verdana";
            initialCssInfo.HeadingStyleFlags["h2"] = new Models.HeadingStyleFlags { IsItalic = true };
            _viewModel.LoadStyles(initialCssInfo); // ViewModelの内部Dictionaryを初期化

            _mockFileService.Setup(s => s.ReadAllText(filePath)).Returns(initialCss);
            _mockCssEditorService.Setup(s => s.UpdateCssContent(initialCss, It.Is<Models.CssStyleInfo>(info => 
                info.BodyTextColor == _viewModel.BodyTextColor && 
                info.BodyBackgroundColor == _viewModel.BodyBackgroundColor &&
                // HeadingTextColorsの検証
                info.HeadingTextColors.ContainsKey("h1") && info.HeadingTextColors["h1"] == initialCssInfo.HeadingTextColors["h1"] &&
                info.HeadingTextColors.ContainsKey("h2") && info.HeadingTextColors["h2"] == initialCssInfo.HeadingTextColors["h2"] &&
                // HeadingFontSizesの検証
                info.HeadingFontSizes.ContainsKey("h1") && info.HeadingFontSizes["h1"] == initialCssInfo.HeadingFontSizes["h1"] &&
                info.HeadingFontSizes.ContainsKey("h2") && info.HeadingFontSizes["h2"] == initialCssInfo.HeadingFontSizes["h2"] &&
                // HeadingFontFamiliesの検証
                info.HeadingFontFamilies.ContainsKey("h1") && info.HeadingFontFamilies["h1"] == initialCssInfo.HeadingFontFamilies["h1"] &&
                info.HeadingFontFamilies.ContainsKey("h2") && info.HeadingFontFamilies["h2"] == initialCssInfo.HeadingFontFamilies["h2"] &&
                // HeadingStyleFlagsの検証
                info.HeadingStyleFlags.ContainsKey("h1") && info.HeadingStyleFlags["h1"].IsBold == initialCssInfo.HeadingStyleFlags["h1"].IsBold &&
                info.HeadingStyleFlags.ContainsKey("h2") && info.HeadingStyleFlags["h2"].IsItalic == initialCssInfo.HeadingStyleFlags["h2"].IsItalic
                )))
                                 .Returns(updatedCss);

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.ReadAllText(filePath), Times.Once);
            _mockCssEditorService.Verify(s => s.UpdateCssContent(initialCss, It.Is<Models.CssStyleInfo>(info =>
                info.BodyTextColor == _viewModel.BodyTextColor &&
                info.BodyBackgroundColor == _viewModel.BodyBackgroundColor &&
                // HeadingTextColorsの検証
                info.HeadingTextColors.ContainsKey("h1") && info.HeadingTextColors["h1"] == initialCssInfo.HeadingTextColors["h1"] &&
                info.HeadingTextColors.ContainsKey("h2") && info.HeadingTextColors["h2"] == initialCssInfo.HeadingTextColors["h2"] &&
                // HeadingFontSizesの検証
                info.HeadingFontSizes.ContainsKey("h1") && info.HeadingFontSizes["h1"] == initialCssInfo.HeadingFontSizes["h1"] &&
                info.HeadingFontSizes.ContainsKey("h2") && info.HeadingFontSizes["h2"] == initialCssInfo.HeadingFontSizes["h2"] &&
                // HeadingFontFamiliesの検証
                info.HeadingFontFamilies.ContainsKey("h1") && info.HeadingFontFamilies["h1"] == initialCssInfo.HeadingFontFamilies["h1"] &&
                info.HeadingFontFamilies.ContainsKey("h2") && info.HeadingFontFamilies["h2"] == initialCssInfo.HeadingFontFamilies["h2"] &&
                // HeadingStyleFlagsの検証
                info.HeadingStyleFlags.ContainsKey("h1") && info.HeadingStyleFlags["h1"].IsBold == initialCssInfo.HeadingStyleFlags["h1"].IsBold &&
                info.HeadingStyleFlags.ContainsKey("h2") && info.HeadingStyleFlags["h2"].IsItalic == initialCssInfo.HeadingStyleFlags["h2"].IsItalic
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

        [TestMethod]
        public void SelectedHeadingLevel_ShouldUpdateHeadingFontSizeAndFamilyAndRaisePropertyChanged()
        {
            // テスト観点: SelectedHeadingLevel プロパティが変更されたときに、
            // HeadingFontSize と HeadingFontFamily が更新され、PropertyChanged イベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo();
            cssInfo.HeadingFontSizes["h1"] = "20px";
            cssInfo.HeadingFontFamilies["h1"] = "Arial";
            cssInfo.HeadingFontSizes["h2"] = "24px";
            cssInfo.HeadingFontFamilies["h2"] = "Verdana";

            // LoadStylesが呼ばれたときに内部状態が設定されるようにモックを設定
            _viewModel.LoadStyles(cssInfo);

            bool fontSizeRaised = false;
            bool fontFamilyRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.HeadingFontSize))
                {
                    fontSizeRaised = true;
                }
                if (args.PropertyName == nameof(CssEditorViewModel.HeadingFontFamily))
                {
                    fontFamilyRaised = true;
                }
            };

            // Act
            _viewModel.SelectedHeadingLevel = "h2";

            // Assert
            Assert.IsTrue(fontSizeRaised, "PropertyChanged for HeadingFontSize should have been raised.");
            Assert.AreEqual("24px", _viewModel.HeadingFontSize);

            Assert.IsTrue(fontFamilyRaised, "PropertyChanged for HeadingFontFamily should have been raised.");
            Assert.AreEqual("Verdana", _viewModel.HeadingFontFamily);
        }

        [TestMethod]
        public void SelectedHeadingLevel_ShouldUpdateHeadingStyleFlagsAndRaisePropertyChanged()
        {
            // テスト観点: SelectedHeadingLevel プロパティが変更されたときに、
            // IsHeadingBold, IsHeadingItalic, IsHeadingUnderline, IsHeadingStrikethrough が更新され、
            // PropertyChanged イベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo();
            cssInfo.HeadingStyleFlags["h1"] = new Models.HeadingStyleFlags { IsBold = true, IsItalic = false, IsUnderline = true, IsStrikethrough = false };
            cssInfo.HeadingStyleFlags["h2"] = new Models.HeadingStyleFlags { IsBold = false, IsItalic = true, IsUnderline = false, IsStrikethrough = true };

            // LoadStylesが呼ばれたときに内部状態が設定されるようにモックを設定
            _viewModel.LoadStyles(cssInfo);

            bool isBoldRaised = false;
            bool isItalicRaised = false;
            bool isUnderlineRaised = false;
            bool isStrikethroughRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(CssEditorViewModel.IsHeadingBold))
                {
                    isBoldRaised = true;
                }
                if (args.PropertyName == nameof(CssEditorViewModel.IsHeadingItalic))
                {
                    isItalicRaised = true;
                }
                if (args.PropertyName == nameof(CssEditorViewModel.IsHeadingUnderline))
                {
                    isUnderlineRaised = true;
                }
                if (args.PropertyName == nameof(CssEditorViewModel.IsHeadingStrikethrough))
                {
                    isStrikethroughRaised = true;
                }
            };

            // Act
            _viewModel.SelectedHeadingLevel = "h2";

            // Assert
            Assert.IsTrue(isBoldRaised, "PropertyChanged for IsHeadingBold should have been raised.");
            Assert.IsFalse(_viewModel.IsHeadingBold);

            Assert.IsTrue(isItalicRaised, "PropertyChanged for IsHeadingItalic should have been raised.");
            Assert.IsTrue(_viewModel.IsHeadingItalic);

            Assert.IsTrue(isUnderlineRaised, "PropertyChanged for IsHeadingUnderline should have been raised.");
            Assert.IsFalse(_viewModel.IsHeadingUnderline);

            Assert.IsTrue(isStrikethroughRaised, "PropertyChanged for IsHeadingStrikethrough should have been raised.");
            Assert.IsTrue(_viewModel.IsHeadingStrikethrough);
        }

        [TestMethod]
        public void LoadStyles_ShouldLoadQuoteStylesAndRaisePropertyChanged()
        {
            // テスト観点: LoadStylesメソッドが呼ばれた際に、CssStyleInfoの引用関連のスタイルが
            // ViewModelの各プロパティに正しく読み込まれ、PropertyChangedイベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo
            {
                QuoteTextColor = "#111111",
                QuoteBackgroundColor = "#222222",
                QuoteBorderWidth = "1px",
                QuoteBorderStyle = "dashed",
                QuoteBorderColor = "#333333"
            };

            var raisedProperties = new System.Collections.Generic.List<string>();
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };

            // Act
            _viewModel.LoadStyles(cssInfo);

            // Assert
            Assert.AreEqual(cssInfo.QuoteTextColor, _viewModel.QuoteTextColor);
            Assert.AreEqual(cssInfo.QuoteBackgroundColor, _viewModel.QuoteBackgroundColor);
            Assert.AreEqual(cssInfo.QuoteBorderWidth, _viewModel.QuoteBorderWidth);
            Assert.AreEqual(cssInfo.QuoteBorderStyle, _viewModel.QuoteBorderStyle);
            Assert.AreEqual(cssInfo.QuoteBorderColor, _viewModel.QuoteBorderColor);

            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.QuoteTextColor)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.QuoteBackgroundColor)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.QuoteBorderWidth)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.QuoteBorderStyle)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.QuoteBorderColor)));
        }

        [TestMethod]
        public void LoadStyles_ShouldLoadListStylesAndRaisePropertyChanged()
        {
            // テスト観点: LoadStylesメソッドが呼ばれた際に、CssStyleInfoのリスト関連のスタイルが
            // ViewModelの各プロパティに正しく読み込まれ、PropertyChangedイベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo
            {
                ListMarkerType = "disc",
                ListIndent = "20px"
            };

            var raisedProperties = new System.Collections.Generic.List<string>();
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };

            // Act
            _viewModel.LoadStyles(cssInfo);

            // Assert
            Assert.AreEqual(cssInfo.ListMarkerType, _viewModel.ListMarkerType);
            Assert.AreEqual(cssInfo.ListIndent, _viewModel.ListIndent);

            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.ListMarkerType)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.ListIndent)));
        }

        [TestMethod]
        public void LoadStyles_ShouldLoadTableStylesAndRaisePropertyChanged()
        {
            // テスト観点: LoadStylesメソッドが呼ばれた際に、CssStyleInfoの表関連のスタイルが
            // ViewModelの各プロパティに正しく読み込まれ、PropertyChangedイベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo
            {
                TableBorderColor = "#111111",
                TableHeaderBackgroundColor = "#222222",
                TableBorderWidth = "2px",
                TableCellPadding = "10px"
            };

            var raisedProperties = new System.Collections.Generic.List<string>();
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };

            // Act
            _viewModel.LoadStyles(cssInfo);

            // Assert
            Assert.AreEqual(cssInfo.TableBorderColor, _viewModel.TableBorderColor);
            Assert.AreEqual(cssInfo.TableHeaderBackgroundColor, _viewModel.TableHeaderBackgroundColor);
            Assert.AreEqual(cssInfo.TableBorderWidth, _viewModel.TableBorderWidth);
            Assert.AreEqual(cssInfo.TableCellPadding, _viewModel.TableCellPadding);

            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.TableBorderColor)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.TableHeaderBackgroundColor)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.TableBorderWidth)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.TableCellPadding)));
        }

        [TestMethod]
        public void LoadStyles_ShouldLoadCodeStylesAndRaisePropertyChanged()
        {
            // テスト観点: LoadStylesメソッドが呼ばれた際に、CssStyleInfoのコード関連のスタイルが
            // ViewModelの各プロパティに正しく読み込まれ、PropertyChangedイベントが発火することを確認する。
            // Arrange
            var cssInfo = new Models.CssStyleInfo
            {
                CodeTextColor = "#111111",
                CodeBackgroundColor = "#222222",
                CodeFontFamily = "Consolas"
            };

            var raisedProperties = new System.Collections.Generic.List<string>();
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != null)
                {
                    raisedProperties.Add(args.PropertyName);
                }
            };

            // Act
            _viewModel.LoadStyles(cssInfo);

            // Assert
            Assert.AreEqual(cssInfo.CodeTextColor, _viewModel.CodeTextColor);
            Assert.AreEqual(cssInfo.CodeBackgroundColor, _viewModel.CodeBackgroundColor);
            Assert.AreEqual(cssInfo.CodeFontFamily, _viewModel.CodeFontFamily);

            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.CodeTextColor)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.CodeBackgroundColor)));
            Assert.IsTrue(raisedProperties.Contains(nameof(CssEditorViewModel.CodeFontFamily)));
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCopyQuoteStylesToCssStyleInfo()
        {
            // テスト観点: SaveCssCommandが実行された際に、ViewModelの引用関連プロパティが
            // CssStyleInfoオブジェクトに正しくコピーされることを確認する。
            // Arrange
            var filePath = "C:\\temp\\test.css";
            _viewModel.TargetCssPath = filePath;

            _viewModel.QuoteTextColor = "#123456";
            _viewModel.QuoteBackgroundColor = "#abcdef";
            _viewModel.QuoteBorderColor = "#fedcba";
            _viewModel.QuoteBorderWidth = "2px";
            _viewModel.QuoteBorderStyle = "dotted";

            Models.CssStyleInfo? capturedStyleInfo = null;
            _mockCssEditorService.Setup(s => s.UpdateCssContent(It.IsAny<string>(), It.IsAny<Models.CssStyleInfo>()))
                                 .Callback<string, Models.CssStyleInfo>((css, styleInfo) => capturedStyleInfo = styleInfo)
                                 .Returns(""); // Return a dummy string

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            Assert.IsNotNull(capturedStyleInfo);
            Assert.AreEqual(_viewModel.QuoteTextColor, capturedStyleInfo.QuoteTextColor);
            Assert.AreEqual(_viewModel.QuoteBackgroundColor, capturedStyleInfo.QuoteBackgroundColor);
            Assert.AreEqual(_viewModel.QuoteBorderColor, capturedStyleInfo.QuoteBorderColor);
            Assert.AreEqual(_viewModel.QuoteBorderWidth, capturedStyleInfo.QuoteBorderWidth);
            Assert.AreEqual(_viewModel.QuoteBorderStyle, capturedStyleInfo.QuoteBorderStyle);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCopyListStylesToCssStyleInfo()
        {
            // テスト観点: SaveCssCommandが実行された際に、ViewModelのリスト関連プロパティが
            // CssStyleInfoオブジェクトに正しくコピーされることを確認する。
            // Arrange
            var filePath = "C:\\temp\\test.css";
            _viewModel.TargetCssPath = filePath;

            _viewModel.ListMarkerType = "square";
            _viewModel.ListIndent = "30px";

            Models.CssStyleInfo? capturedStyleInfo = null;
            _mockCssEditorService.Setup(s => s.UpdateCssContent(It.IsAny<string>(), It.IsAny<Models.CssStyleInfo>()))
                                 .Callback<string, Models.CssStyleInfo>((css, styleInfo) => capturedStyleInfo = styleInfo)
                                 .Returns(""); // Return a dummy string

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            Assert.IsNotNull(capturedStyleInfo);
            Assert.AreEqual(_viewModel.ListMarkerType, capturedStyleInfo.ListMarkerType);
            Assert.AreEqual(_viewModel.ListIndent, capturedStyleInfo.ListIndent);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCopyTableStylesToCssStyleInfo()
        {
            // テスト観点: SaveCssCommandが実行された際に、ViewModelの表関連プロパティが
            // CssStyleInfoオブジェクトに正しくコピーされることを確認する。
            // Arrange
            var filePath = "C:\\temp\\test.css";
            _viewModel.TargetCssPath = filePath;

            _viewModel.TableBorderColor = "#111111";
            _viewModel.TableBorderWidth = "3px";
            _viewModel.TableHeaderBackgroundColor = "#222222";
            _viewModel.TableCellPadding = "15px";

            Models.CssStyleInfo? capturedStyleInfo = null;
            _mockCssEditorService.Setup(s => s.UpdateCssContent(It.IsAny<string>(), It.IsAny<Models.CssStyleInfo>()))
                                 .Callback<string, Models.CssStyleInfo>((css, styleInfo) => capturedStyleInfo = styleInfo)
                                 .Returns(""); // Return a dummy string

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            Assert.IsNotNull(capturedStyleInfo);
            Assert.AreEqual(_viewModel.TableBorderColor, capturedStyleInfo.TableBorderColor);
            Assert.AreEqual(_viewModel.TableBorderWidth, capturedStyleInfo.TableBorderWidth);
            Assert.AreEqual(_viewModel.TableHeaderBackgroundColor, capturedStyleInfo.TableHeaderBackgroundColor);
            Assert.AreEqual(_viewModel.TableCellPadding, capturedStyleInfo.TableCellPadding);
        }

        [TestMethod]
        public void SaveCssCommand_ShouldCopyCodeStylesToCssStyleInfo()
        {
            // テスト観点: SaveCssCommandが実行された際に、ViewModelのコード関連プロパティが
            // CssStyleInfoオブジェクトに正しくコピーされることを確認する。
            // Arrange
            var filePath = "C:\\temp\\test.css";
            _viewModel.TargetCssPath = filePath;

            _viewModel.CodeTextColor = "#dddddd";
            _viewModel.CodeBackgroundColor = "#eeeeee";
            _viewModel.CodeFontFamily = "Courier New";

            Models.CssStyleInfo? capturedStyleInfo = null;
            _mockCssEditorService.Setup(s => s.UpdateCssContent(It.IsAny<string>(), It.IsAny<Models.CssStyleInfo>()))
                                 .Callback<string, Models.CssStyleInfo>((css, styleInfo) => capturedStyleInfo = styleInfo)
                                 .Returns(""); // Return a dummy string

            // Act
            _viewModel.SaveCssCommand.Execute(null);

            // Assert
            Assert.IsNotNull(capturedStyleInfo);
            Assert.AreEqual(_viewModel.CodeTextColor, capturedStyleInfo.CodeTextColor);
            Assert.AreEqual(_viewModel.CodeBackgroundColor, capturedStyleInfo.CodeBackgroundColor);
            Assert.AreEqual(_viewModel.CodeFontFamily, capturedStyleInfo.CodeFontFamily);
        }
    }
}
