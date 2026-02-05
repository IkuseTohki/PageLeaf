using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Linq;
using System;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;

namespace PageLeaf.UITests;

[TestClass]
public class MainWindowTests
{
    private static void WaitUntilEnabled(AutomationElement element, int timeoutMs = 5000)
    {
        var start = DateTime.Now;
        while (!element.IsEnabled)
        {
            if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
                throw new TimeoutException($"Element '{element.AutomationId}' did not become enabled within {timeoutMs}ms.");
            Thread.Sleep(100);
        }
    }

    private static AutomationElement FindGlobal(FlaUI.Core.AutomationBase automation, string automationId, int timeoutMs = 10000)
    {
        var start = DateTime.Now;
        while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
        {
            var element = automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element != null) return element;
            Thread.Sleep(500);
        }
        throw new Exception($"Global element with AutomationId '{automationId}' not found.");
    }

    private static void SetNumericValue(FlaUI.Core.AutomationBase automation, AutomationElement parent, string newValue)
    {
        var input = parent.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit).Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox)));
        if (input == null) throw new Exception("Numeric input not found.");

        input.Focus();
        Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL, FlaUI.Core.WindowsAPI.VirtualKeyShort.KEY_A);
        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.BACK);
        Keyboard.Type(newValue);
        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
        Thread.Sleep(500);

        // 別の場所をクリックしてフォーカスを確実に外す
        Mouse.Click(automation.GetDesktop().GetClickablePoint());
        Thread.Sleep(500);
    }

    private static string GetFullValue(AutomationElement parent)
    {
        // 読込反映待ち
        Thread.Sleep(1000);
        var input = parent.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit).Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox)));
        if (input == null) return "NOT_FOUND";

        if (input.ControlType == FlaUI.Core.Definitions.ControlType.ComboBox)
        {
            return input.AsComboBox().EditableText;
        }
        return input.AsTextBox().Text;
    }

    private static void SelectItem(ComboBox comboBox, string itemName)
    {
        comboBox.Focus();
        Thread.Sleep(500);
        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.HOME);
        Thread.Sleep(200);
        Keyboard.Type(itemName);
        Thread.Sleep(500);
        Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
        Thread.Sleep(2000); // 十分な読込待機
    }

    /// <summary>
    /// テスト観点: アプリケーションが正常に起動し、メインウィンドウのタイトルに正しいアプリケーション名が含まれていることを確認する。
    /// </summary>
    [TestMethod]
    public void AppLaunch_ShouldHaveCorrectTitle()
    {
        // Arrange & Act
        using var session = new PageLeafSession();

        // Assert
        Assert.IsNotNull(session.MainWindow, "メインウィンドウが見つかりません。");

        var title = session.MainWindow!.Title;
        StringAssert.Contains(title, "PageLeaf", $"タイトル '{title}' に 'PageLeaf' が含まれていません。");
    }

    /// <summary>
    /// テスト観点: CSSエディターパネルでの値の変更（フォントサイズ）と保存操作により、実際のCSSファイルの内容が正しく更新されることを確認する。
    /// </summary>
    [TestMethod]
    public void AppGeneratedCss_Lifecycle_Test()
    {
        var uniqueName = $"life_{Guid.NewGuid():N}.css";
        var testDir = AppDomain.CurrentDomain.BaseDirectory;
        var cssPath = Path.Combine(testDir, "css", uniqueName);

        if (!Directory.Exists(Path.Combine(testDir, "css"))) Directory.CreateDirectory(Path.Combine(testDir, "css"));
        File.WriteAllText(cssPath, "body { font-size: 14px; }");

        try
        {
            using var session = new PageLeafSession();
            var automation = session.Automation;

            var toggleButton = FindGlobal(automation, "ToggleCssEditorButton").AsButton();
            toggleButton.Patterns.Invoke.Pattern.Invoke();
            Thread.Sleep(1000);

            var fileSelector = FindGlobal(automation, "CssFileSelector").AsComboBox();
            SelectItem(fileSelector, uniqueName);

            var generalTab = FindGlobal(automation, "GeneralTab").AsRadioButton();
            generalTab.Patterns.SelectionItem.Pattern.Select();
            Thread.Sleep(1000);

            var fontSizeEditor = FindGlobal(automation, "BodyFontSizeEditor");
            var newValue = "35"; // 目立つ値に
            SetNumericValue(automation, fontSizeEditor, newValue);

            var saveButton = FindGlobal(automation, "SaveCssButton").AsButton();
            WaitUntilEnabled(saveButton);
            saveButton.Invoke();
            Thread.Sleep(3000);

            var content = File.ReadAllText(cssPath);
            StringAssert.Contains(content, $"font-size: {newValue}px;", "編集内容が保存されていません。");
        }
        finally
        {
            if (File.Exists(cssPath)) File.Delete(cssPath);
        }
    }

    /// <summary>
    /// テスト観点: 編集対象のCSSファイルを切り替えた際、エディターパネル内の入力項目の表示値が、選択したファイルの内容に基づいて正しく更新されることを確認する。
    /// </summary>
    [TestMethod]
    public void ExternalCss_SwitchFile_ShouldUpdateValues()
    {
        var file1 = "switch_test_1.css";
        var file2 = "switch_test_2.css";
        var testDir = AppDomain.CurrentDomain.BaseDirectory;
        var path1 = Path.Combine(testDir, "css", file1);
        var path2 = Path.Combine(testDir, "css", file2);

        File.WriteAllText(path1, "body { font-size: 11px; }");
        File.WriteAllText(path2, "body { font-size: 22px; }");

        try
        {
            using var session = new PageLeafSession();
            var automation = session.Automation;
            var toggleButton = FindGlobal(automation, "ToggleCssEditorButton").AsButton();
            toggleButton.Patterns.Invoke.Pattern.Invoke();
            Thread.Sleep(1000);

            var fileSelector = FindGlobal(automation, "CssFileSelector").AsComboBox();

            SelectItem(fileSelector, file1);
            var val1 = GetFullValue(FindGlobal(automation, "BodyFontSizeEditor"));

            SelectItem(fileSelector, file2);
            var val2 = GetFullValue(FindGlobal(automation, "BodyFontSizeEditor"));

            Assert.AreNotEqual(val1, val2, $"ファイル切り替え後に設定値が変化していません。 (Val1: {val1}, Val2: {val2})");
        }
        finally
        {
            if (File.Exists(path1)) File.Delete(path1);
            if (File.Exists(path2)) File.Delete(path2);
        }
    }
}
