using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class FrontMatterPropertyTemplateSelectorTests
    {
        private FrontMatterPropertyTemplateSelector _selector = null!;
        private DataTemplate _defaultTemplate = null!;
        private DataTemplate _tagsTemplate = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // Prepare mock templates (using real DataTemplate objects as they are simple containers)
            _defaultTemplate = new DataTemplate();
            _tagsTemplate = new DataTemplate();

            _selector = new FrontMatterPropertyTemplateSelector
            {
                DefaultTemplate = _defaultTemplate,
                TagsTemplate = _tagsTemplate
            };
        }

        [TestMethod]
        public void SelectTemplate_ShouldReturnTagsTemplate_WhenPropertyIsTags()
        {
            /*
            テスト観点:
            IsTagsプロパティがTrueのFrontMatterPropertyが渡された場合、
            TagsTemplateが選択されて返されることを確認する。
            */
            // Arrange
            var prop = new FrontMatterProperty { Key = "tags" }; // Key="tags" implies IsTags=true

            // Act
            var result = _selector.SelectTemplate(prop, new DependencyObject());

            // Assert
            Assert.AreSame(_tagsTemplate, result);
        }

        [TestMethod]
        public void SelectTemplate_ShouldReturnDefaultTemplate_WhenPropertyIsNotTags()
        {
            /*
            テスト観点:
            IsTagsプロパティがFalseのFrontMatterProperty（通常のプロパティ）が渡された場合、
            DefaultTemplateが選択されて返されることを確認する。
            */
            // Arrange
            var prop = new FrontMatterProperty { Key = "title" };

            // Act
            var result = _selector.SelectTemplate(prop, new DependencyObject());

            // Assert
            Assert.AreSame(_defaultTemplate, result);
        }

        [TestMethod]
        public void SelectTemplate_ShouldReturnNull_WhenItemIsNotFrontMatterProperty()
        {
            /*
            テスト観点:
            FrontMatterProperty以外のオブジェクト（またはnull）が渡された場合、
            テンプレートを選択せず null (base.SelectTemplateの結果) を返すことを確認する。
            */
            // Arrange
            var item = new object();

            // Act
            var result = _selector.SelectTemplate(item, new DependencyObject());

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SelectTemplate_ShouldReturnNull_WhenItemIsNull()
        {
            /*
            テスト観点:
            アイテムとしてnullが渡された場合、nullを返すことを確認する。
            */
            // Arrange & Act
            var result = _selector.SelectTemplate(null!, new DependencyObject());

            // Assert
            Assert.IsNull(result);
        }
    }
}
