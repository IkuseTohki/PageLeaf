using System.Diagnostics;
using System.Linq;
using System.IO;
using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace PageLeaf.UITests;

/// <summary>
/// UIテスト用のアプリケーションセッションを管理します。
/// アプリケーションの起動、環境のバックアップ、およびリソースの解放を担当します。
/// </summary>
public class PageLeafSession : IDisposable
{
    private bool _disposed;
    private readonly string _appDir;
    private readonly string _backupDir;

    /// <summary>
    /// 実行中のアプリケーションインスタンス。
    /// </summary>
    public Application App { get; }

    /// <summary>
    /// UI Automation エンジン。
    /// </summary>
    public UIA3Automation Automation { get; }

    /// <summary>
    /// アプリケーションのメインウィンドウ。
    /// </summary>
    public Window? MainWindow { get; }

    /// <summary>
    /// CSS ファイルが格納されているディレクトリのパス。
    /// </summary>
    public string CssDirectoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css");

    /// <summary>
    /// 新しいセッションを開始し、アプリケーションを起動します。
    /// </summary>
    public PageLeafSession()
    {
        Automation = new UIA3Automation();

        // 実行ファイルのパスを決定
        var appPath = GetApplicationPath();
        _appDir = Path.GetDirectoryName(Path.GetFullPath(appPath)) ?? throw new Exception("Could not get app directory.");

        // テスト環境のディレクトリ
        var testBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        _backupDir = Path.Combine(Path.GetTempPath(), "PageLeaf_Test_Backup_" + Guid.NewGuid().ToString("N"));

        // CSSフォルダのバックアップ (冪等性の確保)
        BackupDirectory(Path.Combine(testBaseDir, "css"), _backupDir);

        if (!File.Exists(appPath))
        {
            throw new FileNotFoundException($"Application executable not found at: {appPath}");
        }

        // WebView2 Loader DLL の問題を避けるため、本来の出力ディレクトリから DLL をコピーする
        EnsureWebView2Loader(appPath);

        var psi = new ProcessStartInfo(appPath)
        {
            WorkingDirectory = _appDir
        };
        App = Application.Launch(psi);

        // メインウィンドウが表示されるまで待機（タイムアウト20秒）
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(20));

        if (MainWindow == null)
        {
            var allWindows = App.GetAllTopLevelWindows(Automation);
            MainWindow = allWindows.FirstOrDefault(w => w.Title.Contains("PageLeaf"));
        }
    }

    private void BackupDirectory(string source, string target)
    {
        if (!Directory.Exists(source)) return;
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
        }
    }

    private void RestoreDirectory(string source, string target)
    {
        if (!Directory.Exists(source)) return;
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
        }
        // バックアップにないファイル（テストで作成されたファイル）を削除
        var backupFiles = Directory.GetFiles(source).Select(Path.GetFileName).ToHashSet();
        foreach (var file in Directory.GetFiles(target))
        {
            if (!backupFiles.Contains(Path.GetFileName(file)) && !Path.GetFileName(file).EndsWith(".txt")) // ログ以外
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    private void EnsureWebView2Loader(string appPath)
    {
        var targetDir = Path.GetDirectoryName(appPath)!;
        var loaderName = "WebView2Loader.dll";
        var targetLoaderPath = Path.Combine(targetDir, loaderName);

        if (!File.Exists(targetLoaderPath))
        {
            // runtimes から探してコピー
            var sourceLoaderPath = Path.Combine(targetDir, "runtimes", "win-x64", "native", loaderName);
            if (File.Exists(sourceLoaderPath))
            {
                File.Copy(sourceLoaderPath, targetLoaderPath, true);
            }
        }
    }

    private static string GetApplicationPath()
    {
        var testDir = AppDomain.CurrentDomain.BaseDirectory;

        // テストディレクトリにある exe を優先（依存 DLL の問題が解決されている場合）
        var localPath = Path.Combine(testDir, "PageLeaf.exe");
        if (File.Exists(localPath)) return localPath;

        // 開発環境のパス
        string[] candidates =
        {
            Path.Combine(testDir, "..", "..", "..", "..", "PageLeaf", "bin", "Debug", "net8.0-windows", "win-x64", "PageLeaf.exe"),
            Path.Combine(testDir, "..", "..", "..", "..", "PageLeaf", "bin", "Debug", "net8.0-windows", "PageLeaf.exe"),
        };

        foreach (var path in candidates)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath)) return fullPath;
        }

        throw new FileNotFoundException("Application executable not found.");
    }

    /// <summary>
    /// アプリケーションを終了し、セッションを閉じます。
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        try { Automation.Dispose(); } catch { }

        try
        {
            int pid = App.ProcessId;
            App.Close();
            // 終了を待機
            using var process = Process.GetProcessById(pid);
            if (!process.WaitForExit(3000)) process.Kill();
        }
        catch
        {
            try { Process.GetProcessById(App.ProcessId).Kill(); } catch { }
        }

        // CSSフォルダのリストア
        try { RestoreDirectory(_backupDir, CssDirectoryPath); Directory.Delete(_backupDir, true); } catch { }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
