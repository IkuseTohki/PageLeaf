using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private Mock<IFileService> _mockFileService = null!;
        private Mock<ILogger<MainViewModel>> _mockLogger = null!;
        private Mock<IDialogService> _mockDialogService = null!;
        private Mock<IEditorService> _mockEditorService = null!;
        private Mock<ICssService> _mockCssService = null!;
        private Mock<ISettingsService> _mockSettingsService = null!;
        private Mock<ICssEditorService> _mockCssEditorService = null!;
        private MainViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockFileService = new Mock<IFileService>();
            _mockLogger = new Mock<ILogger<MainViewModel>>();
            _mockDialogService = new Mock<IDialogService>();
            _mockEditorService = new Mock<IEditorService>();
            _mockCssService = new Mock<ICssService>();
            _mockSettingsService = new Mock<ISettingsService>();
            _mockCssEditorService = new Mock<ICssEditorService>();

            // Default setup for _mockCssService
            var defaultCssFiles = new List<string> { "theme1.css", "theme2.css" };
            _mockCssService.Setup(s => s.GetAvailableCssFileNames()).Returns(defaultCssFiles);

            // Default setup for _mockSettingsService
            var initialSettings = new ApplicationSettings();
            _mockSettingsService.Setup(s => s.LoadSettings()).Returns(initialSettings);
            _mockSettingsService.SetupGet(s => s.CurrentSettings).Returns(initialSettings); // Setup CurrentSettings property
        }

        [TestMethod]
        public void OpenFileCommand_ShouldCallEditorServiceLoadDocument_WhenFileSelected()
        {
            // テスト観点: OpenFileCommand が実行され、ファイルが選択された際に、
            //             IEditorService の LoadDocument メソッドが正しいドキュメントで呼び出されることを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string testFilePath = @"C:\test\test.md";
            var markdownDocument = new MarkdownDocument { FilePath = testFilePath, Content = "# Test" };

            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(testFilePath);
            _mockFileService.Setup(s => s.Open(testFilePath))
                            .Returns(markdownDocument);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.LoadDocument(markdownDocument), Times.Once);
        }

        [TestMethod]
        public void OpenFileCommand_ShouldDoNothing_WhenDialogIsCancelled()
        {
            // テスト観点: ファイル選択ダイアログがキャンセルされた場合、IEditorService のメソッドが何も呼ばれないことを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns((string?)null);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.LoadDocument(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void OpenFileCommand_ShouldLogExceptionAndDoNothing_WhenFileServiceThrowsException()
        {
            // テスト観点: ファイル読み込みに失敗した場合、エラーがログ記録され、IEditorService のメソッドが呼ばれないことを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string testFilePath = @"C:\test\test.md";
            var exception = new System.IO.FileNotFoundException("File not found");

            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(testFilePath);
            _mockFileService.Setup(s => s.Open(testFilePath))
                            .Throws(exception);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.LoadDocument(It.IsAny<MarkdownDocument>()), Times.Never);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to open file")),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenFileServiceIsNull()
        {
            // テスト観点: IFileService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(null!, _mockLogger.Object, _mockDialogService.Object, _mockEditorService.Object, _mockCssService.Object, _mockSettingsService.Object, _mockCssEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // テスト観点: ILogger が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, null!, _mockDialogService.Object, _mockEditorService.Object, _mockCssService.Object, _mockSettingsService.Object, _mockCssEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDialogServiceIsNull()
        {
            // テスト観点: IDialogService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, null!, _mockEditorService.Object, _mockCssService.Object, _mockSettingsService.Object, _mockCssEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenEditorServiceIsNull()
        {
            // テスト観点: IEditorService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, null!, _mockCssService.Object, _mockSettingsService.Object, _mockCssEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenCssEditorServiceIsNull()
        {
            // テスト観点: ICssEditorService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, _mockEditorService.Object, _mockCssService.Object, _mockSettingsService.Object, null!));
        }

        [TestMethod]
        public void SaveFileCommand_ShouldCallFileServiceSave_WhenDocumentHasFilePath()
        {
            // テスト観点: SaveFileCommand が、Editor.CurrentDocument の内容を IFileService.Save を介して上書き保存することを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string testFilePath = @"C:\test\existing.md";
            string fileContent = "# Existing Content";
            var documentToSave = new MarkdownDocument { FilePath = testFilePath, Content = fileContent };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(testFilePath)).Returns(true); // ファイルが存在するとモック

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc => doc.FilePath == testFilePath && doc.Content == fileContent)), Times.Once);
        }

        [TestMethod]
        public void SaveAsFileCommand_ShouldCallFileServiceSave_WhenUserSelectsFilePath()
        {
            // テスト観点: SaveAsFileCommand が、正しい内容と新しいファイルパスで IFileService.Save を呼び出すことを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string initialContent = "# Initial Content";
            var initialDocument = new MarkdownDocument { Content = initialContent };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(initialDocument);

            string newFilePath = @"C:\new\path\to\file.md";
            _mockDialogService.Setup(s => s.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
                              .Returns(newFilePath);

            // Act
            _viewModel.SaveAsFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc =>
                doc.FilePath == newFilePath && doc.Content == initialContent)), Times.Once);
        }

        [TestMethod]
        public void ExecuteSaveFile_WhenFileDoesNotExist_ShouldCallExecuteSaveAsFile()
        {
            // テスト観点: Editor.CurrentDocument.FilePath が存在しない場合、ExecuteSaveFile が ExecuteSaveAsFile を呼び出すことを検証する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string nonExistentFilePath = @"C:\nonexistent\file.md";
            var documentToSave = new MarkdownDocument { FilePath = nonExistentFilePath, Content = "# New Content" };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(nonExistentFilePath)).Returns(false); // ファイルが存在しないとモック

            // ExecuteSaveAsFile が呼び出されたことを検証するためのモック
            // SaveAsFileCommand は DelegateCommand なので、直接 Verify できない。
            // そのため、SaveAsFileCommand が内部で呼び出す ShowSaveFileDialog をモックし、それが呼び出されることを検証する。
            _mockDialogService.Setup(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            )).Returns((string?)null); // ダイアログはキャンセルされたと仮定

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            // ExecuteSaveAsFile が呼び出されたことを間接的に検証
            _mockDialogService.Verify(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            ), Times.Once);
            // ファイルが存在しないため、_fileService.Save は呼び出されないことを確認
            _mockFileService.Verify(s => s.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void ExecuteSaveFile_WhenFileDoesNotExistAndSaveAsIsCanceled_ShouldNotSave()
        {
            // テスト観点: Editor.CurrentDocument.FilePath が存在せず、ExecuteSaveAsFile 内の ShowSaveFileDialog がキャンセルされた場合、_fileService.Save が呼び出されないことを検証する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string nonExistentFilePath = @"C:\nonexistent\file.md";
            var documentToSave = new MarkdownDocument { FilePath = nonExistentFilePath, Content = "# New Content" };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(nonExistentFilePath)).Returns(false); // ファイルが存在しないとモック

            _mockDialogService.Setup(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            )).Returns((string?)null); // ダイアログはキャンセルされたと仮定

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void ExecuteSaveFile_WhenFileDoesNotExistAndSaveAsIsSuccessful_ShouldSaveWithNewPath()
        {
            // テスト観点: Editor.CurrentDocument.FilePath が存在せず、ExecuteSaveAsFile 内の ShowSaveFileDialog で新しいパスが選択された場合、Editor.CurrentDocument.FilePath が更新され、_fileService.Save が新しいパスで呼び出されることを検証する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            string nonExistentFilePath = @"C:\nonexistent\file.md";
            string newFilePath = @"C:\new\path\to\saved_file.md";
            string fileContent = "# New Content";
            var documentToSave = new MarkdownDocument { FilePath = nonExistentFilePath, Content = fileContent };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(nonExistentFilePath)).Returns(false); // ファイルが存在しないとモック

            _mockDialogService.Setup(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            )).Returns(newFilePath); // 新しいパスが選択されたと仮定

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            // Editor.CurrentDocument.FilePath が新しいパスに更新されていることを確認
            Assert.AreEqual(newFilePath, _mockEditorService.Object.CurrentDocument.FilePath);
            // _fileService.Save が新しいパスで呼び出されたことを確認
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc =>
                doc.FilePath == newFilePath && doc.Content == fileContent)), Times.Once);
        }

        [TestMethod]
        public void test_MainViewModel_ShouldLoadCssFilesFromServiceOnInitialization()
        {
            // テスト観点: MainViewModel が初期化された際に、ICssService から利用可能なCSSファイルリストが正しくロードされることを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            var availableCss = new List<string> { "theme1.css", "theme2.css" };
            _mockCssService.Setup(s => s.GetAvailableCssFileNames()).Returns(availableCss);

            // Act
            // ViewModelはSetupで初期化済みなので、ここでは再初期化しない
            // _viewModel = new MainViewModel(...);

            // Assert
            CollectionAssert.AreEquivalent(availableCss, _viewModel.AvailableCssFiles.ToList());
            _mockCssService.Verify(s => s.GetAvailableCssFileNames(), Times.Once);
        }

        [TestMethod]
        public void test_MainViewModel_ShouldSelectDefaultCssFile_OnInitialization()
        {
            // テスト観点: MainViewModel が初期化された際に、利用可能なCSSファイルリストの最初の要素がデフォルトとして選択されることを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            var availableCss = new List<string> { "theme1.css", "theme2.css" };
            _mockCssService.Setup(s => s.GetAvailableCssFileNames()).Returns(availableCss);

            // Act
            // ViewModelはSetupで初期化済みなので、ここでは再初期化しない
            // _viewModel = new MainViewModel(...);

            // Assert
            Assert.AreEqual("theme1.css", _viewModel.SelectedCssFile);
            _mockCssService.Verify(s => s.GetAvailableCssFileNames(), Times.Once);
        }

        [TestMethod]
        public void test_MainViewModel_ShouldLoadSelectedCssFromSettingsOnInitialization()
        {
            // テスト観点: MainViewModel が初期化された際に、設定サービスから保存されたCSSテーマが正しくロードされることを確認する。
            // Arrange
            var savedCss = "theme2.css";
            var settings = new ApplicationSettings { SelectedCss = savedCss };
            _mockSettingsService.Setup(s => s.LoadSettings()).Returns(settings);
            _mockSettingsService.SetupGet(s => s.CurrentSettings).Returns(settings); // Also setup CurrentSettings

            var availableCss = new List<string> { "theme1.css", savedCss, "theme3.css" };
            _mockCssService.Setup(s => s.GetAvailableCssFileNames()).Returns(availableCss);

            // Act
            // ViewModelを再初期化して、モックの設定が反映されるようにする
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);

            // Assert
            Assert.AreEqual(savedCss, _viewModel.SelectedCssFile);
        }

        [TestMethod]
        public void test_MainViewModel_ShouldSaveSelectedCssToSettingsWhenChanged()
        {
            // テスト観点: SelectedCssFile プロパティが変更された際に、その新しい値が設定サービスを通じて保存されることを確認する。
            // Arrange
            var initialSettings = new ApplicationSettings { SelectedCss = "theme1.css" };
            _mockSettingsService.Setup(s => s.LoadSettings()).Returns(initialSettings);
            _mockSettingsService.SetupGet(s => s.CurrentSettings).Returns(initialSettings);

            ApplicationSettings? savedSettings = null;
            _mockSettingsService.Setup(s => s.SaveSettings(It.IsAny<ApplicationSettings>()))
                              .Callback<ApplicationSettings>(s => savedSettings = s);

            var availableCss = new List<string> { "theme1.css", "theme2.css" };
            _mockCssService.Setup(s => s.GetAvailableCssFileNames()).Returns(availableCss);

            // Re-initialize ViewModel to use the specific settings for this test
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);

            // Act
            _viewModel.SelectedCssFile = "theme2.css";

            // Assert
            _mockSettingsService.Verify(s => s.SaveSettings(It.IsAny<ApplicationSettings>()), Times.Exactly(2)); // Constructor + Setter
            Assert.IsNotNull(savedSettings);
            Assert.AreEqual("theme2.css", savedSettings.SelectedCss);
        }

        [TestMethod]
        public void SelectedCssFile_WhenChanged_ShouldCallApplyCssOnEditorService()
        {
            // テスト観点: SelectedCssFile プロパティが変更された際に、IEditorService の ApplyCss メソッドが正しいファイル名で呼び出されることを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);

            var newCss = "new-theme.css";

            // Act
            _viewModel.SelectedCssFile = newCss;

            // Assert
            _mockEditorService.Verify(s => s.ApplyCss(newCss), Times.Once);
        }

        [TestMethod]
        public void NewDocumentCommand_ShouldCallEditorServiceNewDocument()
        {
            // テスト観点: NewDocumentCommand が実行された際に、IEditorService の NewDocument メソッドが呼び出されることを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);

            // Act
            _viewModel.NewDocumentCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.NewDocument(), Times.Once);
        }

        [TestMethod]
        public void IsCssEditorVisible_ShouldBeFalse_ByDefault()
        {
            // テスト観点: IsCssEditorVisible プロパティのデフォルト値が false であることを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);

            // Assert
            Assert.IsFalse(_viewModel.IsCssEditorVisible);
        }

        [TestMethod]
        public void IsCssEditorVisible_ShouldRaisePropertyChanged_WhenChanged()
        {
            // テスト観点: IsCssEditorVisible プロパティが変更されたときに、PropertyChanged イベントが発火することを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);

            bool wasRaised = false;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.IsCssEditorVisible))
                {
                    wasRaised = true;
                }
            };

            // Act
            _viewModel.IsCssEditorVisible = true;

            // Assert
            Assert.IsTrue(wasRaised);
        }

        [TestMethod]
        public void ToggleCssEditorCommand_ShouldToggleIsCssEditorVisible()
        {
            // テスト観点: ToggleCssEditorCommand を実行すると、IsCssEditorVisible プロパティの値が反転することを確認する。
            // Arrange
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object,
                _mockCssService.Object,
                _mockSettingsService.Object,
                _mockCssEditorService.Object);
            var initialValue = _viewModel.IsCssEditorVisible;

            // Act
            _viewModel.ToggleCssEditorCommand.Execute(null);

            // Assert
            Assert.AreEqual(!initialValue, _viewModel.IsCssEditorVisible);

            // Act
            _viewModel.ToggleCssEditorCommand.Execute(null);

                        // Assert

                        Assert.AreEqual(initialValue, _viewModel.IsCssEditorVisible);

                    }

            

                    [TestMethod]

                    public void SelectedCssFile_WhenChanged_ShouldParseCssAndSetToCssEditorViewModel()

                    {

                        // テスト観点: SelectedCssFileが変更された際、CSSファイルを読み込み、解析し、

                        //             その結果がCssEditorViewModelに正しく設定されることを確認する。

                        // Arrange

                        var cssFileName = "test.css";

                        var cssFilePath = "C:\\css\\test.css";

                        var cssContent = "body { color: #111111; background-color: #eeeeee; }";

                        var parsedStyles = new CssStyleInfo { BodyTextColor = "rgba(17, 17, 17, 1)", BodyBackgroundColor = "rgba(238, 238, 238, 1)" };

            

                                    _mockCssService.Setup(s => s.GetCssPath(cssFileName)).Returns(cssFilePath);

            

                                    _mockFileService.Setup(s => s.FileExists(cssFilePath)).Returns(true);

            

                                    _mockFileService.Setup(s => s.ReadAllText(cssFilePath)).Returns(cssContent);

                        _mockCssEditorService.Setup(s => s.ParseCss(cssContent)).Returns(parsedStyles);

            

                        _viewModel = new MainViewModel(

                            _mockFileService.Object,

                            _mockLogger.Object,

                            _mockDialogService.Object,

                            _mockEditorService.Object,

                            _mockCssService.Object,

                            _mockSettingsService.Object,

                            _mockCssEditorService.Object);

            

                        // Act

                        _viewModel.SelectedCssFile = cssFileName;

            

                        // Assert

                        _mockFileService.Verify(s => s.ReadAllText(cssFilePath), Times.Once);

                        _mockCssEditorService.Verify(s => s.ParseCss(cssContent), Times.Once);

                        Assert.AreEqual(parsedStyles.BodyTextColor, _viewModel.CssEditorViewModel.BodyTextColor);

                        Assert.AreEqual(parsedStyles.BodyBackgroundColor, _viewModel.CssEditorViewModel.BodyBackgroundColor);

                    }

                }

            }

            