namespace PageLeaf.UseCases
{
    /// <summary>
    /// 現在のドキュメントを上書き保存するユースケースのインターフェースです。
    /// </summary>
    public interface ISaveDocumentUseCase
    {
        /// <summary>
        /// ドキュメントを保存します。ファイルパスが未設定の場合は「名前を付けて保存」を呼び出します。
        /// </summary>
        /// <returns>保存が成功した場合は true、失敗した場合は false。</returns>
        bool Execute();
    }
}