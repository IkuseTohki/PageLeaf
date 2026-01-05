using System;
using System.IO;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// 画像保存時のファイル名生成やパス解決を行うユーティリティクラスです。
    /// </summary>
    public static class ImageFileNameResolver
    {
        /// <summary>
        /// テンプレートに基づいてファイル名を生成します。
        /// </summary>
        /// <param name="template">ファイル名テンプレート。</param>
        /// <param name="markdownFilePath">現在のMarkdownファイルのパス。</param>
        /// <param name="now">日時。</param>
        /// <returns>解決されたファイル名（拡張子なし）。</returns>
        public static string ResolveFileName(string template, string markdownFilePath, DateTime now)
        {
            var fileName = Path.GetFileNameWithoutExtension(markdownFilePath);
            var result = template
                .Replace("{Date}", now.ToString("yyyyMMdd"))
                .Replace("{Time}", now.ToString("HHmmss"))
                .Replace("{FileName}", fileName);

            return result;
        }

        /// <summary>
        /// 画像を保存する絶対パスを解決します。
        /// </summary>
        /// <param name="markdownFilePath">Markdownファイルのパス。</param>
        /// <param name="relativeSaveDir">保存先ディレクトリ（Markdownファイルからの相対パス）。</param>
        /// <param name="fileName">ファイル名（拡張子付き）。</param>
        /// <returns>絶対パス。</returns>
        public static string ResolveFullSavePath(string markdownFilePath, string relativeSaveDir, string fileName)
        {
            var markdownDir = Path.GetDirectoryName(markdownFilePath);
            if (string.IsNullOrEmpty(markdownDir))
            {
                // Markdownファイルが未保存の場合などはカレントディレクトリを基準にするか、例外にするなどの考慮が必要だが、
                // いったん空の場合はそのまま返すか、アプリの実行ディレクトリを基準にする。
                // ここでは呼び出し元でMarkdownファイルのパスが確定している前提とする。
                return Path.Combine(relativeSaveDir, fileName);
            }

            var saveDir = Path.Combine(markdownDir, relativeSaveDir);
            return Path.GetFullPath(Path.Combine(saveDir, fileName));
        }
    }
}
