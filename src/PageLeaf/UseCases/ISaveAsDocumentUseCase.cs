namespace PageLeaf.UseCases
{
    /// <summary>
    /// 現在のドキュメントに名前を付けて保存するユースケースのインターフェースです。
    /// </summary>
    public interface ISaveAsDocumentUseCase
    {
        /// <summary>
        /// 保存先を選択するダイアログを表示し、ドキュメントを保存します。
        /// </summary>
        /// <returns>保存が成功した場合は true、キャンセルまたは失敗した場合は false。</returns>
        bool Execute();
    }
}