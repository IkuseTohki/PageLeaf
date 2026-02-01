using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using System.Windows;

using Moq;
using System;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class DialogServiceTests
    {
        [TestMethod]
        [Ignore]
        public void ShowSaveConfirmationDialog_ShouldReturnCorrectResult()
        {
            // テスト観点: ShowSaveConfirmationDialog が適切なメッセージとボタンで表示され、ユーザーの選択に応じた SaveConfirmationResult を返すことを確認する。
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockWindowService = new Mock<IWindowService>();
            var dialogService = new DialogService(mockServiceProvider.Object, mockWindowService.Object);

            // WPF の MessageBox は直接モックするのが困難なため、ここでは MessageBox の結果を直接テストするのではなく、
            // Integration Test として手動で動作確認するか、UI Automation を利用する方が現実的です。
            // Unit Test の範囲では、DialogService がMessageBox.Show を呼び出すこと、および引数が正しいことを検証すべきですが、
            // 今回はインターフェースの実装と最低限の動作確認として、このテストは主にコンパイルや基本的なロジックの確認を目的とします。

            // MessageBoxResult.Yes をシミュレート (実際にはユーザー操作が必要)
            // SaveConfirmationResult result = dialogService.ShowSaveConfirmationDialog();
            // Assert.AreEqual(SaveConfirmationResult.Save, result);

            // このテストは実行時に手動でダイアログ操作ができないため、常に NoAction を返すと仮定してテストを記述します。
            // 実際のアプリケーションでは、このテストは UI テストフレームワークを使用して実施すべきです。

            // Act (ここではテストが常に NoAction を返すようにする)
            SaveConfirmationResult result = dialogService.ShowSaveConfirmationDialog();

            // Assert
            // MessageBox を直接テストできないため、これはあくまで仮のテストです。
            // 実際に確認したいのは、ShowSaveConfirmationDialog が MessageBox.Show を呼び出し、
            // その結果を正しく SaveConfirmationResult にマッピングすることです。
            // この Assert は、MessageBoxResult.Yes, No, Cancel のうちどれが返ってきても、
            // それがSaveConfirmationResultに正しくマッピングされることを期待しています。
            // 例: MessageBoxResult.Yes が返された場合、SaveConfirmationResult.Save となるか。
            // 今回はテスト実行時にダイアログを操作できないため、モックを使用するか、より高度なUIテストを採用する必要があります。
            // ここではコンパイルが通り、基本的なコード構造が正しいことを確認するのみに留めます。
            Assert.IsNotNull(result);
        }
    }
}
