using System.Threading.Tasks;

namespace PageLeaf.Services
{
    /// <summary>
    /// クリップボードからの画像貼り付け処理を行うサービスです。
    /// </summary>
    public interface IImagePasteService
    {
        /// <summary>
        /// クリップボード内の画像を保存します。
        /// </summary>
        /// <param name="directoryPath">保存先ディレクトリの絶対パス。</param>
        /// <param name="fileNameWithoutExtension">拡張子なしのファイル名。</param>
        /// <returns>保存されたファイルの絶対パス。画像がない場合は null。</returns>
        Task<string?> SaveClipboardImageAsync(string directoryPath, string fileNameWithoutExtension);
    }
}
