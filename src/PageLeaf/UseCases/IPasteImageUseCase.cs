using System.Threading.Tasks;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// 画像貼り付けユースケースのインターフェースです。
    /// </summary>
    public interface IPasteImageUseCase
    {
        /// <summary>
        /// 画像貼り付けを実行します。
        /// </summary>
        /// <param name="currentMarkdownFilePath">現在のMarkdownファイルのパス。</param>
        Task ExecuteAsync(string currentMarkdownFilePath);
    }
}
