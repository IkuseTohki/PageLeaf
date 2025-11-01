using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;

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
            var fileService = new FileService(new NullLogger<FileService>());

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
        [TestMethod]
        public void OpenFolder_ShouldThrowDirectoryNotFoundException_WhenPathDoesNotExist()
        {
            // テスト観点: 存在しないフォルダパスを指定した場合、DirectoryNotFoundExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var nonExistentPath = Path.Combine(_testFolderPath, "non_existent_folder");

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(() => fileService.OpenFolder(nonExistentPath));
        }
        [TestMethod]
        public void OpenFolder_ShouldReturnEmptyCollection_WhenFolderIsEmpty()
        {
            // テスト観点: 空のフォルダを指定した場合、空のコレクションが返されることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var emptyFolderPath = Path.Combine(_testFolderPath, "empty_folder");
            Directory.CreateDirectory(emptyFolderPath);

            // Act
            var result = fileService.OpenFolder(emptyFolderPath).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        [DataTestMethod]
        [DataRow("", DisplayName = "空文字列")]
        [DataRow("   ", DisplayName = "空白文字列")]
        public void OpenFolder_ShouldThrowArgumentException_WhenPathIsInvalid(string invalidPath)
        {
            // テスト観点: 無効な（空や空白の）文字列がパスとして指定された場合、ArgumentExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => fileService.OpenFolder(invalidPath));
        }

        [TestMethod]
        public void OpenFolder_ShouldThrowArgumentNullException_WhenPathIsNull()
        {
            // テスト観点: nullがパスとして指定された場合、ArgumentNullExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => fileService.OpenFolder(null!));
        }

        [TestMethod]
        public void Save_ShouldWriteContentToFile_WhenDocumentIsValid()
        {
            // テスト観点: Saveメソッドが、指定されたファイルパスに正しい内容を書き込むことを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var filePath = Path.Combine(_testFolderPath, "new_file.md");
            var content = "# Hello World\nThis is a test.";
            var document = new MarkdownDocument { FilePath = filePath, Content = content };

            // Act
            fileService.Save(document);

            // Assert
            Assert.IsTrue(File.Exists(filePath), "ファイルが作成されていません。");
            var savedContent = File.ReadAllText(filePath);
            Assert.AreEqual(content, savedContent, "ファイルの内容が正しくありません。");
        }

        [TestMethod]
        public void Save_ShouldThrowArgumentNullException_WhenDocumentIsNull()
        {
            // テスト観点: Saveメソッドにnullを渡した場合、ArgumentNullExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => fileService.Save(null!));
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "FilePath is null")]
        [DataRow("", DisplayName = "FilePath is empty")]
        [DataRow("   ", DisplayName = "FilePath is whitespace")]
        public void Save_ShouldThrowArgumentException_WhenFilePathIsInvalid(string invalidPath)
        {
            // テスト観点: FilePathが無効な場合にArgumentExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var document = new MarkdownDocument { FilePath = invalidPath, Content = "some content" };

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => fileService.Save(document));
        }

        [TestMethod]
        public void Open_ShouldReturnCorrectDocument_WhenFileExists()
        {
            // テスト観点: Openメソッドが、指定されたファイルの内容を正しく読み込み、MarkdownDocumentとして返すことを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var filePath = Path.Combine(_testFolderPath, "file1.txt");
            var expectedContent = "content1";

            // Act
            var document = fileService.Open(filePath);

            // Assert
            Assert.IsNotNull(document);
            Assert.AreEqual(filePath, document.FilePath);
            Assert.AreEqual(expectedContent, document.Content);
        }

        [TestMethod]
        public void Open_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // テスト観点: 存在しないファイルをOpenしようとした場合、FileNotFoundExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var nonExistentFilePath = Path.Combine(_testFolderPath, "non_existent_file.txt");

            // Act & Assert
            Assert.ThrowsException<FileNotFoundException>(() => fileService.Open(nonExistentFilePath));
        }

        [DataTestMethod]
        [DataRow(null, DisplayName = "FilePath is null")]
        [DataRow("", DisplayName = "FilePath is empty")]
        public void Open_ShouldThrowArgumentException_WhenPathIsInvalid(string invalidPath)
        {
            // テスト観点: FilePathが無効な場合にArgumentExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => fileService.Open(invalidPath));
        }
    }
}
