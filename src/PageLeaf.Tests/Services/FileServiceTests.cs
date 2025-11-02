using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

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

        public static IEnumerable<object[]> InvalidPaths =>
            new List<object[]>
            {
                new object[] { null! },
                new object[] { "" }
            };

        public static IEnumerable<object[]> GetFilesTestCases =>
            new List<object[]>
            {
                new object[] { "*.txt", new string[] { "file1.txt", "file3.txt" } },
                new object[] { "*.md", new string[] { "file4.md" } },
                new object[] { "*.*", new string[] { "file1.txt", "file3.txt", "file4.md", "non_css_file.log" } }
            };

        [DataTestMethod]
        [DynamicData(nameof(InvalidPaths))]
        public void Open_ShouldThrowArgumentException_WhenPathIsInvalid(string invalidPath)
        {
            // テスト観点: FilePathが無効な場合にArgumentExceptionがスローされることを確認する。

            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => fileService.Open(invalidPath));
        }

        [TestMethod]
        public void Open_ShouldDetectShiftJisEncoding_WhenFileIsShiftJis()
        {
            // テスト観点: Shift_JISでエンコードされたファイルを正しく読み込めることを確認する。
            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var filePath = Path.Combine(_testFolderPath, "sjis_file.md");
            var expectedContent = "これはShift_JISのテストです。";

            // Shift_JIS (code page 932) のエンコーディングを取得
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjisEncoding = Encoding.GetEncoding(932);

            File.WriteAllText(filePath, expectedContent, sjisEncoding);

            // Act
            var document = fileService.Open(filePath);

            // Assert
            Assert.AreEqual(expectedContent, document.Content, "ファイルの内容が正しくデコードされていません。");
        }

        [TestMethod]
        public void Save_ShouldPreserveOriginalShiftJisEncoding()
        {
            // テスト観点: Shift_JISでエンコードされたファイルを開き、変更して保存した際に、
            //             元のShift_JISエンコーディングが維持されることを確認する。
            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var filePath = Path.Combine(_testFolderPath, "sjis_to_save.md");
            var originalContent = "これはShift_JISのテストです。";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjisEncoding = Encoding.GetEncoding(932);
            File.WriteAllText(filePath, originalContent, sjisEncoding);

            // Act
            // ファイルを開き、内容を変更して保存
            var document = fileService.Open(filePath);
            document.Content += " (変更済み)";
            fileService.Save(document);

            // Assert
            // 保存されたファイルのエンコーディングを再確認
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var charsetDetector = new Ude.CharsetDetector();
                charsetDetector.Feed(fileStream);
                charsetDetector.DataEnd();

                Assert.IsNotNull(charsetDetector.Charset, "エンコーディングが判別できませんでした。");
                Assert.AreEqual("SHIFT-JIS", charsetDetector.Charset.ToUpper(), "エンコーディングがShift-JISに維持されていません。");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(GetFilesTestCases))]
        public void GetFiles_ShouldReturnCorrectListOfFiles_WhenFolderContainsMatchingFiles(string searchPattern, string[] expectedFileNames)
        {
            // テスト観点: 指定されたフォルダに一致するファイルが存在する場合、正しいファイル名のリストが返されることを検証する。
            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var testSubFolder = Path.Combine(_testFolderPath, "test_getfiles_folder");
            Directory.CreateDirectory(testSubFolder);
            File.WriteAllText(Path.Combine(testSubFolder, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(testSubFolder, "file3.txt"), "content3");
            File.WriteAllText(Path.Combine(testSubFolder, "file4.md"), "content4");
            File.WriteAllText(Path.Combine(testSubFolder, "non_css_file.log"), "log content");

            // Act
            var result = fileService.GetFiles(testSubFolder, searchPattern).ToList();

            // Assert
            Assert.AreEqual(expectedFileNames.Length, result.Count, "返されたファイル数が期待値と異なります。");
            foreach (var expectedFileName in expectedFileNames)
            {
                Assert.IsTrue(result.Any(f => Path.GetFileName(f) == expectedFileName), $"{expectedFileName} が結果に含まれていません。");
            }
        }

        [TestMethod]
        public void GetFiles_ShouldReturnEmptyList_WhenFolderIsEmpty()
        {
            // テスト観点: 空のフォルダを指定した場合、空のリストが返されることを検証する。
            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var emptyFolder = Path.Combine(_testFolderPath, "empty_getfiles_folder");
            Directory.CreateDirectory(emptyFolder);

            // Act
            var result = fileService.GetFiles(emptyFolder, "*.*").ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count, "空のフォルダからファイルが返されました。");
        }

        [TestMethod]
        public void GetFiles_ShouldReturnEmptyList_WhenFolderDoesNotExist()
        {
            // テスト観点: 存在しないフォルダを指定した場合、空のリストが返されることを検証する。
            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var nonExistentFolder = Path.Combine(_testFolderPath, "non_existent_getfiles_folder");

            // Act
            var result = fileService.GetFiles(nonExistentFolder, "*.*").ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count, "存在しないフォルダからファイルが返されました。");
        }

        [TestMethod]
        public void GetFiles_ShouldNotIncludeNonMatchingFiles()
        {
            // テスト観点: 指定されたパターンに一致しないファイルが含まれていても、一致するファイルのみがリストに含まれることを検証する。
            // Arrange
            var fileService = new FileService(new NullLogger<FileService>());
            var testSubFolder = Path.Combine(_testFolderPath, "test_non_matching_files");
            Directory.CreateDirectory(testSubFolder);
            File.WriteAllText(Path.Combine(testSubFolder, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(testSubFolder, "image.png"), "binary content");
            File.WriteAllText(Path.Combine(testSubFolder, "document.docx"), "binary content");

            // Act
            var result = fileService.GetFiles(testSubFolder, "*.txt").ToList();

            // Assert
            Assert.AreEqual(1, result.Count, "返されたファイル数が期待値と異なります。");
            Assert.IsTrue(result.Any(f => Path.GetFileName(f) == "file1.txt"), "file1.txt が結果に含まれていません。");
            Assert.IsFalse(result.Any(f => Path.GetFileName(f) == "image.png"), "image.png が誤って含まれています。");
            Assert.IsFalse(result.Any(f => Path.GetFileName(f) == "document.docx"), "document.docx が誤って含まれています。");
        }
    }
}
