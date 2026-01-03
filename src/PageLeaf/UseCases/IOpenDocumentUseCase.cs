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
    }
}