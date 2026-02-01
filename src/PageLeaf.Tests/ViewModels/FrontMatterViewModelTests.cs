using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class FrontMatterViewModelTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private MarkdownDocument _document = null!;
        private FrontMatterViewModel _viewModel = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _document = new MarkdownDocument();
            _document.FrontMatter = new Dictionary<string, object>
            {
                { "title", "Test Title" },
                { "created", "2023-01-01" },
                { "tags", new List<object> { "tag1", "tag2" } },
                { "custom", "customValue" }
            };

            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(_document);

            _viewModel = new FrontMatterViewModel(_editorServiceMock.Object);
        }

        [TestMethod]
        public void Constructor_ShouldLoadPropertiesFromDocument()
        {
            /*
            テスト観点:
            ViewModelの初期化時に、DocumentのFrontMatter情報が正しくプロパティリストにロードされるか確認する。
            - 通常のプロパティ(title)が反映されること
            - リスト形式のプロパティ(tags)がTagsコレクションとして反映されること
            */
            // Assert
            Assert.AreEqual(4, _viewModel.Properties.Count);

            var titleProp = _viewModel.Properties.First(p => p.Key == "title");
            Assert.AreEqual("Test Title", titleProp.Value);

            var tagsProp = _viewModel.Properties.First(p => p.Key == "tags");
            Assert.IsNotNull(tagsProp.Tags);
            Assert.AreEqual(2, tagsProp.Tags.Count);
            Assert.AreEqual("tag1", tagsProp.Tags[0]);
        }

        [TestMethod]
        public void PropertyChanged_ShouldUpdateDocument()
        {
            /*
            テスト観点:
            ViewModel上のプロパティ値を変更した際、同期してDocumentのFrontMatterの値も更新されるか確認する。
            */
            // Arrange
            var customProp = _viewModel.Properties.First(p => p.Key == "custom");

            // Act
            customProp.Value = "newValue";

            // Assert
            Assert.AreEqual("newValue", _document.FrontMatter["custom"]);
        }

        [TestMethod]
        public void AddPropertyCommand_ShouldAddPropertyAndSyncToDocument()
        {
            /*
            テスト観点:
            AddPropertyCommandを実行した際、新しいプロパティがViewModelに追加され、
            同時にDocumentのFrontMatterにもキーが追加されるか確認する。
            */
            // Act
            _viewModel.AddPropertyCommand.Execute(null);

            // Assert
            Assert.AreEqual(5, _viewModel.Properties.Count);
            var newProp = _viewModel.Properties.Last();
            Assert.AreEqual("new_property", newProp.Key);

            // ドキュメントへの同期確認
            Assert.IsTrue(_document.FrontMatter.ContainsKey("new_property"));
        }

        [TestMethod]
        public void RemovePropertyCommand_ShouldRemovePropertyAndSyncToDocument()
        {
            /*
            テスト観点:
            RemovePropertyCommandを実行した際、対象のプロパティがViewModelから削除され、
            DocumentのFrontMatterからもキーが削除されるか確認する。
            */
            // Arrange
            var customProp = _viewModel.Properties.First(p => p.Key == "custom");

            // Act
            _viewModel.RemovePropertyCommand.Execute(customProp);

            // Assert
            Assert.AreEqual(3, _viewModel.Properties.Count);
            Assert.IsFalse(_viewModel.Properties.Contains(customProp));

            // ドキュメントへの同期確認
            Assert.IsFalse(_document.FrontMatter.ContainsKey("custom"));
        }

        [TestMethod]
        public void AddTagCommand_ShouldAddTagAndSyncToDocument()
        {
            /*
            テスト観点:
            AddTagCommandを実行した際、指定したタグがTagsコレクションに追加され、
            DocumentのFrontMatter内のリストも更新されるか確認する。
            */
            // Arrange
            var tagsProp = _viewModel.Properties.First(p => p.Key == "tags");
            tagsProp.NewTagText = "tag3";

            // Act
            _viewModel.AddTagCommand.Execute(tagsProp);

            // Assert
            Assert.AreEqual(3, tagsProp.Tags!.Count);
            Assert.IsTrue(tagsProp.Tags.Contains("tag3"));
            Assert.AreEqual(string.Empty, tagsProp.NewTagText);

            // ドキュメントへの同期確認
            var newTags = _document.FrontMatter["tags"] as IEnumerable<string>;
            Assert.IsNotNull(newTags);
            Assert.IsTrue(newTags.Contains("tag3"));
        }

        [TestMethod]
        public void RemoveTagCommand_ShouldRemoveTagAndSyncToDocument()
        {
            /*
            テスト観点:
            RemoveTagCommandを実行した際、指定したタグがTagsコレクションから削除され、
            DocumentのFrontMatter内のリストからも削除されるか確認する。
            */
            // Arrange
            var tagsProp = _viewModel.Properties.First(p => p.Key == "tags");
            var tagToRemove = "tag1";
            var args = new object[] { tagsProp, tagToRemove };

            // Act
            _viewModel.RemoveTagCommand.Execute(args);

            // Assert
            Assert.AreEqual(1, tagsProp.Tags!.Count);
            Assert.IsFalse(tagsProp.Tags.Contains(tagToRemove));

            // ドキュメントへの同期確認
            var newTags = _document.FrontMatter["tags"] as IEnumerable<string>;
            Assert.IsNotNull(newTags);
            Assert.IsFalse(newTags.Contains(tagToRemove));
        }

        [TestMethod]
        public void FrontMatterProperty_ReadOnlyLogic_ShouldBeCorrect()
        {
            /*
            テスト観点:
            特定のキー（title, created, tags）に対して、
            IsKeyReadOnly, IsValueReadOnly, CanRemove などのView制御フラグが
            仕様通りに正しく判定されているか確認する。
            */
            // title: キーは読み取り専用、値は編集可能
            var title = _viewModel.Properties.First(p => p.Key == "title");
            Assert.IsTrue(title.IsKeyReadOnly, "titleのキーは読み取り専用であるべきです");
            Assert.IsFalse(title.IsValueReadOnly, "titleの値は編集可能であるべきです");
            Assert.IsFalse(title.CanRemove, "titleは削除不可であるべきです");

            // created: キー、値ともに読み取り専用
            var created = _viewModel.Properties.First(p => p.Key == "created");
            Assert.IsTrue(created.IsKeyReadOnly);
            Assert.IsTrue(created.IsValueReadOnly);
            Assert.IsFalse(created.CanRemove);

            // custom: キー、値ともに編集可能
            var custom = _viewModel.Properties.First(p => p.Key == "custom");
            Assert.IsFalse(custom.IsKeyReadOnly);
            Assert.IsFalse(custom.IsValueReadOnly);
            Assert.IsTrue(custom.CanRemove);

            // tags: キーは読み取り専用、IsTagsフラグがTrue
            var tags = _viewModel.Properties.First(p => p.Key == "tags");
            Assert.IsTrue(tags.IsKeyReadOnly);
            Assert.IsFalse(tags.CanRemove);
            Assert.IsTrue(tags.IsTags);

            // css: キーは読み取り専用、値は編集可能
            var css = new FrontMatterProperty { Key = "css" };
            Assert.IsTrue(css.IsKeyReadOnly, "cssのキーは読み取り専用であるべきです");
            Assert.IsFalse(css.IsValueReadOnly, "cssの値は編集可能であるべきです");
            Assert.IsFalse(css.CanRemove, "cssは削除不可であるべきです");

            // syntax_highlight: キーは読み取り専用、値は編集可能
            var highlight = new FrontMatterProperty { Key = "syntax_highlight" };
            Assert.IsTrue(highlight.IsKeyReadOnly, "syntax_highlightのキーは読み取り専用であるべきです");
            Assert.IsFalse(highlight.IsValueReadOnly, "syntax_highlightの値は編集可能であるべきです");
            Assert.IsFalse(highlight.CanRemove, "syntax_highlightは削除不可であるべきです");
        }
    }
}
