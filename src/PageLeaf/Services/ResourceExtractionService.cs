using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace PageLeaf.Services
{
    public class ResourceExtractionService : IResourceExtractionService
    {
        private readonly Assembly _assembly;

        public ResourceExtractionService(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public void ExtractAll(string baseDirectory, string tempDirectory)
        {
            var resourceNames = _assembly.GetManifestResourceNames();

            foreach (var resName in resourceNames)
            {
                // リソース名（論理名）のパス区切りをスラッシュに統一して正規化
                // MSBuild の %(RecursiveDir) はバックスラッシュを含むため、ここで統一する
                var normalizedRelPath = resName.Replace('\\', '/');

                if (!IsTargetResource(normalizedRelPath)) continue;

                // 書き出し先のパスを決定（OS標準の区切り文字を使用）
                string relativePathForOS = normalizedRelPath.Replace('/', Path.DirectorySeparatorChar);
                string targetPath;
                bool overwrite = true;

                if (IsInternalResource(normalizedRelPath))
                {
                    // アプリ専用リソース（一時フォルダへ。常に上書き）
                    targetPath = Path.Combine(tempDirectory, relativePathForOS);
                }
                else
                {
                    // ユーザー用成果物（exeフォルダへ。存在しない場合のみ展開）
                    targetPath = Path.Combine(baseDirectory, relativePathForOS);
                    overwrite = false;
                }

                try
                {
                    if (!overwrite && File.Exists(targetPath)) continue;

                    var dir = Path.GetDirectoryName(targetPath);
                    if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    using var stream = _assembly.GetManifestResourceStream(resName);
                    if (stream == null) continue;

                    using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);

                    Debug.WriteLine($"Extracted: {resName} -> {targetPath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to extract resource: {resName} to {targetPath}. Error: {ex.Message}");
                }
            }
        }

        public bool IsInternalResource(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;

            // スラッシュに統一して判定
            var normalized = relativePath.Replace('\\', '/');

            if (normalized.Equals("css/extensions.css", StringComparison.OrdinalIgnoreCase)) return true;
            if (normalized.StartsWith("highlight/", StringComparison.OrdinalIgnoreCase)) return true;
            if (normalized.StartsWith("mermaid/", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private bool IsTargetResource(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;

            // スラッシュに統一して判定
            var normalized = relativePath.Replace('\\', '/');
            return normalized.StartsWith("css/") ||
                   normalized.StartsWith("highlight/") ||
                   normalized.StartsWith("mermaid/");
        }
    }
}
