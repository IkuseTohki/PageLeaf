namespace PageLeaf.UseCases
{
    /// <summary>
    /// 既存のMarkdownファイルを選択して開くユースケースのインターフェースです。
    /// </summary>
    public interface IOpenDocumentUseCase
    {
        /// <summary>
        /// ファイル選択ダイアログを表示し、ドキュメントを読み込みます。
        /// </summary>
        void Execute();

        /// <summary>
        /// 指定されたパスのファイルを開きます。
        /// </summary>
        /// <param name="filePath">開くファイルのパス。</param>
        void OpenPath(string filePath);
    }
}
