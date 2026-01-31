using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using System.Reflection;
using System.IO;
using System;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class ResourceExtractionServiceTests
    {
        private ResourceExtractionService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new ResourceExtractionService(typeof(App).Assembly);
        }

        [TestMethod]
        [DataRow("css/extensions.css", true)]
        [DataRow("css/github.css", false)]
        [DataRow("highlight/highlight.min.js", true)]
        [DataRow("highlight/styles/github.css", true)]
        [DataRow("mermaid/mermaid.min.js", true)]
        [DataRow("js/preview-extensions.js", true)]
        [DataRow("Resources/css.png", false)]
        public void IsInternalResource_ShouldCorrectlyIdentifyInternalResources(string path, bool expected)
        {
            // テスト観点: アプリ内部で管理すべきリソースと、ユーザー成果物として扱うリソースが正しく分類されることを確認する。
            Assert.AreEqual(expected, _service.IsInternalResource(path), $"Failed for path: {path}");
        }

        [TestMethod]
        [DataRow("highlight\\styles\\github.css", true)]
        public void IsInternalResource_ShouldHandleBackslashes(string path, bool expected)
        {
            // テスト観点: Windows形式のバックスラッシュを含むパスでも正しく判定できることを確認する。
            // C#文字列リテラル内でのバックスラッシュ扱いに注意
            Assert.AreEqual(expected, _service.IsInternalResource(path));
        }

        [TestMethod]
        public void ExtractAll_ShouldCreateDirectoriesAndFiles()
        {
            // テスト観点: 実際のリソース展開処理において、ディレクトリが作成されファイルが書き出されることを確認する。

            var baseDir = Path.Combine(Path.GetTempPath(), "PageLeafTests", "Base");
            var tempDir = Path.Combine(Path.GetTempPath(), "PageLeafTests", "Temp");

            try
            {
                if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

                Directory.CreateDirectory(baseDir);
                Directory.CreateDirectory(tempDir);

                // Act
                _service.ExtractAll(baseDir, tempDir);

                // Assert
                // extensions.css は Temp にあるはず
                Assert.IsTrue(File.Exists(Path.Combine(tempDir, "css", "extensions.css")));

                // github.css は Base にあるはず
                Assert.IsTrue(File.Exists(Path.Combine(baseDir, "css", "github.css")));

                // highlight 等のサブフォルダも作成されているはず
                Assert.IsTrue(Directory.Exists(Path.Combine(tempDir, "highlight")));
            }
            finally
            {
                // Cleanup
                // エラー時でも削除できるようにするが、念のためテスト中はパスを確認できるように
                // if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
                // if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }
    }
}
