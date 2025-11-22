using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using System.Windows;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<IDialogService> _dialogServiceMock = null!;
        private Mock<ICssService> _cssServiceMock = null!;
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private Mock<ICssEditorService> _cssEditorServiceMock = null!;
        private Mock<ILogger<MainViewModel>> _loggerMock = null!;
        private MainViewModel _viewModel = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _cssServiceMock = new Mock<ICssService>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _cssEditorServiceMock = new Mock<ICssEditorService>();
            _loggerMock = new Mock<ILogger<MainViewModel>>();

            // MainViewModelのコンストラクタが必要とする基本的な戻り値をセットアップ
            _cssServiceMock.Setup(s => s.GetAvailableCssFileNames()).Returns(new List<string> { "default.css" });
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings { SelectedCss = "default.css" });

            _viewModel = new MainViewModel(
                _fileServiceMock.Object,
                _loggerMock.Object,
                _dialogServiceMock.Object,
                _editorServiceMock.Object,
                _cssServiceMock.Object,
                _settingsServiceMock.Object,
                _cssEditorServiceMock.Object);
        }

        [TestMethod]
        public void ToggleCssEditor_ShouldUpdateColumnWidthAndNotify()
        {
            // テスト観点: IsCssEditorVisibleプロパティの変更に応じて、CssEditorColumnWidthが適切に更新され、
            //            関連するプロパティ変更通知が発行されることを確認する。

            // Arrange
            var notifiedProperties = new List<string>();
            _viewModel.PropertyChanged += (sender, e) => notifiedProperties.Add(e.PropertyName);
            var testWidth = new GridLength(250, GridUnitType.Pixel);
                        _viewModel.CssEditorColumnWidth = testWidth;
            _viewModel.IsCssEditorVisible = false; // 初期状態を非表示に

            notifiedProperties.Clear();

            // Act: 表示をtrueにする
            _viewModel.IsCssEditorVisible = true;

            // Assert: 表示状態
            Assert.IsTrue(_viewModel.IsCssEditorVisible);
            Assert.AreEqual(testWidth, _viewModel.CssEditorColumnWidth);
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.IsCssEditorVisible)));
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.CssEditorColumnWidth)));

            notifiedProperties.Clear();

            // Act: 表示をfalseにする
            _viewModel.IsCssEditorVisible = false;

            // Assert: 非表示状態
            Assert.IsFalse(_viewModel.IsCssEditorVisible);
            Assert.AreEqual(new GridLength(0), _viewModel.CssEditorColumnWidth);
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.IsCssEditorVisible)));
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.CssEditorColumnWidth)));
        }

        [TestMethod]
        public void SetWidth_ShouldBePreservedAfterTogglingVisibility()
        {
            // テスト観点: SetCssEditorColumnWidthで設定した幅が、エディタの表示/非表示を切り替えた後も
            //            正しく保持・復元されることを確認する。

            // Arrange
            var initialWidth = new GridLength(300, GridUnitType.Star);
            var newWidth = new GridLength(450, GridUnitType.Pixel);

            // 初期状態で表示
            _viewModel.IsCssEditorVisible = true;
                        _viewModel.CssEditorColumnWidth = initialWidth;
            Assert.AreEqual(initialWidth, _viewModel.CssEditorColumnWidth, "Initial width should be set.");

            // Act: 新しい幅を設定
                        _viewModel.CssEditorColumnWidth = newWidth;

            // Assert: 新しい幅が適用されている
            Assert.AreEqual(newWidth, _viewModel.CssEditorColumnWidth, "New width should be applied.");

            // Act: 非表示にしてから再度表示
            _viewModel.IsCssEditorVisible = false;
            Assert.AreEqual(new GridLength(0), _viewModel.CssEditorColumnWidth, "Width should be 0 when hidden.");
            _viewModel.IsCssEditorVisible = true;

            // Assert: 新しい幅が復元されている
            Assert.AreEqual(newWidth, _viewModel.CssEditorColumnWidth, "New width should be restored after toggling visibility.");
        }
    }
}