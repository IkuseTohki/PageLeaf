using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System;
using System.IO;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class ImageFileNameResolverTests
    {
        [TestMethod]
        public void ResolveFileName_ShouldReplaceVariables()
        {
            // テスト観点: テンプレート内の変数が正しく置換されることを確認する。
            var template = "img_{Date}_{Time}_{FileName}";
            var markdownFileName = "my-document.md";
            var now = new DateTime(2026, 1, 5, 12, 34, 56);

            var result = ImageFileNameResolver.ResolveFileName(template, markdownFileName, now);

            Assert.AreEqual("img_20260105_123456_my-document", result);
        }

        [TestMethod]
        public void ResolveFileName_WithExtension_ShouldWork()
        {
            // テスト観点: Markdownファイル名が拡張子付きでも正しくベース名が使用されることを確認する。
            var template = "{FileName}_image";
            var markdownFileName = "test.markdown";
            var now = DateTime.Now;

            var result = ImageFileNameResolver.ResolveFileName(template, markdownFileName, now);

            Assert.AreEqual("test_image", result);
        }

        [TestMethod]
        public void ResolveFullSavePath_ShouldReturnAbsolutePath()
        {
            // テスト観点: Markdownファイルの場所を基準とした絶対パスが正しく計算されることを確認する。
            var markdownPath = @"C:\docs\test.md";
            var relativeDir = "assets/images";
            var fileName = "image.png";

            var result = ImageFileNameResolver.ResolveFullSavePath(markdownPath, relativeDir, fileName);

            // Windows環境を想定
            var expected = Path.Combine(@"C:\docs", "assets", "images", "image.png");
            Assert.AreEqual(expected, result);
        }
    }
}
