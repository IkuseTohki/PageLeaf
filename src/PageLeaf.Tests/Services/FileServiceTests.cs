using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class FileServiceTests
    {
        private string _testFolderPath = null!;

        [TestInitialize]
        public void Setup()
        {
            // テスト用の一時フォルダを作成
            _testFolderPath = Path.Combine(Path.GetTempPath(), "PageLeafTestFolder");
            if (Directory.Exists(_testFolderPath))
            {
                Directory.Delete(_testFolderPath, true);
            }
            Directory.CreateDirectory(_testFolderPath);

            // テスト用のファイルとサブフォルダを作成
            File.WriteAllText(Path.Combine(_testFolderPath, "file1.txt"), "content1");
            var subfolder = Directory.CreateDirectory(Path.Combine(_testFolderPath, "subfolder"));
            File.WriteAllText(Path.Combine(subfolder.FullName, "file2.txt"), "content2");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // テスト用の一時フォルダを削除
            if (Directory.Exists(_testFolderPath))
            {
                Directory.Delete(_testFolderPath, true);
            }
        }

        [TestMethod]
        public void OpenFolder_ShouldReturnCorrectTreeStructure()
        {
            // テスト観点: 指定したフォルダの構造を正しくFileTreeNodeのツリーとして返却することを確認する。

            // Arrange
            var fileService = new FileService();

            // Act
            var result = fileService.OpenFolder(_testFolderPath).ToList();

            // Assert
            Assert.AreEqual(2, result.Count, "ルートレベルの項目数が正しくありません。");

            var file1Node = result.FirstOrDefault(n => n.Name == "file1.txt");
            Assert.IsNotNull(file1Node, "file1.txtが見つかりません。");
            Assert.IsFalse(file1Node.IsDirectory, "file1.txtがディレクトリとして扱われています。");
            Assert.AreEqual(Path.Combine(_testFolderPath, "file1.txt"), file1Node.FilePath);

            var subfolderNode = result.FirstOrDefault(n => n.Name == "subfolder");
            Assert.IsNotNull(subfolderNode, "subfolderが見つかりません。");
            Assert.IsTrue(subfolderNode.IsDirectory, "subfolderがファイルとして扱われています。");
            Assert.AreEqual(Path.Combine(_testFolderPath, "subfolder"), subfolderNode.FilePath);

            Assert.IsNotNull(subfolderNode.Children, "subfolderのChildrenがnullです。");
            var subfolderChildren = subfolderNode.Children.ToList();
            Assert.AreEqual(1, subfolderChildren.Count, "subfolder内の項目数が正しくありません。");

            var file2Node = subfolderChildren.FirstOrDefault(n => n.Name == "file2.txt");
            Assert.IsNotNull(file2Node, "file2.txtが見つかりません。");
            Assert.IsFalse(file2Node.IsDirectory, "file2.txtがディレクトリとして扱われています。");
            Assert.AreEqual(Path.Combine(_testFolderPath, "subfolder", "file2.txt"), file2Node.FilePath);
        }
    }
}
