using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class ExternalReloadTests
    {
        [TestMethod]
        public void FileService_Open_ShouldReturnFullContent()
        {
            // 現状の確認: FileService.Open は全文を Content に入れて返すことを確認
            var service = new FileService(new NullLogger<FileService>());
            var path = System.IO.Path.GetTempFileName();
            var content = "---\ntitle: test\n---\n# Body";
            System.IO.File.WriteAllText(path, content);

            try
            {
                var doc = service.Open(path);
                // 現状は全文が入っているはず
                Assert.AreEqual(content, doc.Content);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }
    }
}

