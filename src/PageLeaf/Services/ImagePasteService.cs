using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PageLeaf.Services
{
    public class ImagePasteService : IImagePasteService
    {
        public Task<string?> SaveClipboardImageAsync(string directoryPath, string fileNameWithoutExtension)
        {
            // UIスレッドで実行する必要があるため、Dispatcherを使用
            // 結果を受け取るために TaskCompletionSource を使うか、Invoke の戻り値を使う

            var tcs = new TaskCompletionSource<string?>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    string? savedPath = null;

                    if (Clipboard.ContainsImage())
                    {
                        var image = Clipboard.GetImage();
                        if (image != null)
                        {
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            // PNGとして保存
                            var fileName = fileNameWithoutExtension + ".png";
                            var fullPath = Path.Combine(directoryPath, fileName);

                            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                            {
                                var encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(image));
                                encoder.Save(fileStream);
                            }
                            savedPath = fullPath;
                        }
                    }
                    else if (Clipboard.ContainsFileDropList())
                    {
                        var fileList = Clipboard.GetFileDropList();
                        if (fileList.Count > 0)
                        {
                            var sourceFilePath = fileList[0];
                            // 最初のファイルのみ処理（複数ファイル対応は今後の課題）
                            if (IsImageFile(sourceFilePath) && sourceFilePath != null)
                            {
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                var extension = Path.GetExtension(sourceFilePath);
                                var fileName = fileNameWithoutExtension + extension;
                                var fullPath = Path.Combine(directoryPath, fileName);

                                File.Copy(sourceFilePath, fullPath, true);
                                savedPath = fullPath;
                            }
                        }
                    }

                    tcs.SetResult(savedPath);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private bool IsImageFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp" || ext == ".webp";
        }
    }
}
