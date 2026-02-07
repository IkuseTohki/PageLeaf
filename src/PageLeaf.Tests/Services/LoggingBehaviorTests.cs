using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class LoggingBehaviorTests
    {
        private string _testLogDir = null!;

        [TestInitialize]
        public void Setup()
        {
            _testLogDir = Path.Combine(Path.GetTempPath(), "PageLeafLogTest_" + Guid.NewGuid().ToString());
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testLogDir))
            {
                // ロガーを確実にクローズしてから削除を試みる
                Log.CloseAndFlush();
                // ファイルロックが外れるまでわずかに待機
                Thread.Sleep(100);
                try { Directory.Delete(_testLogDir, true); } catch { /* Ignore */ }
            }
        }

        [TestMethod]
        public void ConditionalFileSink_ShouldNotWrite_WhenFlagIsFalse()
        {
            // テスト観点: Conditional シンクの条件が false の場合、ログがファイルに書き込まれないことを確認する。
            bool enableLogging = false;
            var logFile = Path.Combine(_testLogDir, "test-log.txt");

            using (var logger = new LoggerConfiguration()
                .WriteTo.Conditional(
                    ev => enableLogging,
                    wt => wt.File(logFile))
                .CreateLogger())
            {
                logger.Information("This should not be written.");
            }

            // 注意: WriteTo.File は初期化時にファイルを作成する場合がある。
            // そのため「ファイルが存在しないこと」ではなく「中身が空であること」または
            // 「意図したメッセージが含まれていないこと」を確認する。
            if (File.Exists(logFile))
            {
                var content = File.ReadAllText(logFile);
                Assert.IsFalse(content.Contains("This should not be written."), "Log content should not be written when flag is false.");
            }
        }
    }
}
