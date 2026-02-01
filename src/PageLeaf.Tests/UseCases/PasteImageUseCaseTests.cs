using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.UseCases;
using System.Threading.Tasks;

namespace PageLeaf.Tests.UseCases
{
    [TestClass]
    public class PasteImageUseCaseTests
    {
        private Mock<IImagePasteService> _imagePasteServiceMock = null!;
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<IDialogService> _dialogServiceMock = null!;
        private PasteImageUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _imagePasteServiceMock = new Mock<IImagePasteService>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _editorServiceMock = new Mock<IEditorService>();
            _dialogServiceMock = new Mock<IDialogService>();

            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings());

            _useCase = new PasteImageUseCase(
                _imagePasteServiceMock.Object,
                _settingsServiceMock.Object,
                _editorServiceMock.Object,
                _dialogServiceMock.Object);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldShowMessage_WhenCurrentPathIsEmpty()
        {
            // テスト観点: Markdownファイルが未保存（パスが空）の場合、警告メッセージを表示して処理を中断することを確認する。

            // Act
            await _useCase.ExecuteAsync(string.Empty);

            // Assert
            _dialogServiceMock.Verify(x => x.ShowMessage(It.IsAny<string>(), "画像貼り付け"), Times.Once);
            _imagePasteServiceMock.Verify(x => x.SaveClipboardImageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldInsertMarkdownLink_WhenImageIsSaved()
        {
            // テスト観点: 画像が正常に保存された場合、Markdownエディタに相対パスの画像リンクが挿入されることを確認する。

            // Arrange
            var currentMarkdownPath = @"C:\docs\test.md";
            var savedImagePath = @"C:\docs\images\pasted_image.png";

            _imagePasteServiceMock.Setup(x => x.SaveClipboardImageAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(savedImagePath);

            // Act
            await _useCase.ExecuteAsync(currentMarkdownPath);

            // Assert
            // WindowsパスがMarkdown用のスラッシュに変換されていることも確認
            _editorServiceMock.Verify(x => x.RequestInsertText(It.Is<string>(s => s.Contains("images/pasted_image.png"))), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldNotInsert_WhenSaveFails()
        {
            // テスト観点: 画像の保存に失敗（サービスがnullを返却）した場合、エディタへの挿入が行われないことを確認する。

            // Arrange
            var currentMarkdownPath = @"C:\docs\test.md";
            _imagePasteServiceMock.Setup(x => x.SaveClipboardImageAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            // Act
            await _useCase.ExecuteAsync(currentMarkdownPath);

            // Assert
            _editorServiceMock.Verify(x => x.RequestInsertText(It.IsAny<string>()), Times.Never);
        }
    }
}
