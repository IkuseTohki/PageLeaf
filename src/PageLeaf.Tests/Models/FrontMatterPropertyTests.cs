using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using System.ComponentModel;

namespace PageLeaf.Tests.Models
{
    [TestClass]
    public class FrontMatterPropertyTests
    {
        [TestMethod]
        public void Placeholder_ShouldReturnText_WhenKeyIsTitle()
        {
            // テスト観点: Keyがtitleの場合に適切なプレースホルダーが返されることを確認する。
            var prop = new FrontMatterProperty { Key = "title" };
            Assert.AreEqual("(タイトル未設定)", prop.Placeholder);
        }

        [TestMethod]
        public void Placeholder_ShouldReturnNull_WhenKeyIsNotTitle()
        {
            // テスト観点: title以外のキーではプレースホルダーがnullであることを確認する。
            var prop = new FrontMatterProperty { Key = "author" };
            Assert.IsNull(prop.Placeholder);
        }

        [TestMethod]
        public void Placeholder_ShouldNotifyChange_WhenKeyChanges()
        {
            // テスト観点: Keyを変更した際にPlaceholderプロパティの変更通知が発生することを確認する。
            var prop = new FrontMatterProperty { Key = "author" };
            bool notified = false;
            prop.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FrontMatterProperty.Placeholder))
                {
                    notified = true;
                }
            };

            prop.Key = "title";
            Assert.IsTrue(notified, "Placeholder property change should be notified when Key changes to title.");
        }
    }
}
