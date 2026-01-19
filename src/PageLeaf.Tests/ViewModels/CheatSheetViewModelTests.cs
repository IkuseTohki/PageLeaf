using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.ViewModels;
using System.Linq;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class CheatSheetViewModelTests
    {
        [TestMethod]
        public void Constructor_ShouldInitializeCollections()
        {
            // テスト観点: コンストラクタが呼ばれた際、MarkdownItemsとShortcutItemsがnullでなく、要素を持っていることを確認する。

            // Arrange & Act
            var viewModel = new CheatSheetViewModel();

            // Assert
            Assert.IsNotNull(viewModel.MarkdownItems, "MarkdownItems should not be null");
            Assert.IsNotNull(viewModel.ShortcutItems, "ShortcutItems should not be null");
            Assert.IsTrue(viewModel.MarkdownItems.Any(), "MarkdownItems should contain elements");
            Assert.IsTrue(viewModel.ShortcutItems.Any(), "ShortcutItems should contain elements");
        }

        [TestMethod]
        public void MarkdownItems_ShouldHaveCorrectProperties()
        {
            // テスト観点: Markdownアイテムが正しい構文、カテゴリ、関連ショートカットを持っているか確認する。

            // Arrange
            var viewModel = new CheatSheetViewModel();

            // Act
            var boldItem = viewModel.MarkdownItems.FirstOrDefault(x => x.Description.Contains("太字"));
            var pageBreakItem = viewModel.MarkdownItems.FirstOrDefault(x => x.Description.Contains("改ページ"));

            // Assert
            Assert.IsNotNull(boldItem, "Bold item not found");
            Assert.AreEqual("**太字**", boldItem.Syntax);
            Assert.AreEqual("強調", boldItem.Category);
            Assert.AreEqual("Ctrl + B", boldItem.RelatedShortcut); // 追加した関連ショートカットの検証

            Assert.IsNotNull(pageBreakItem, "Page break item not found");
            Assert.AreEqual("Shift + Enter", pageBreakItem.RelatedShortcut);
            Assert.IsTrue(pageBreakItem.Note.Contains("page-break-after"), "Note should contain HTML tag info");
        }

        [TestMethod]
        public void ShortcutItems_ShouldIncludeNewShortcuts()
        {
            // テスト観点: 追加された見出しショートカットなどがリストに含まれているか確認する。

            // Arrange
            var viewModel = new CheatSheetViewModel();

            // Act
            var headingShortcut = viewModel.ShortcutItems.FirstOrDefault(x => x.Syntax.Contains("Ctrl + 1"));
            var pasteImageShortcut = viewModel.ShortcutItems.FirstOrDefault(x => x.Syntax == "Ctrl + Shift + V");

            // Assert
            Assert.IsNotNull(headingShortcut, "Heading shortcut (Ctrl+1~6) not found");
            Assert.AreEqual("見出し操作", headingShortcut.Category);

            Assert.IsNotNull(pasteImageShortcut, "Paste image shortcut not found");
            Assert.AreEqual("編集操作", pasteImageShortcut.Category);
        }
    }
}
