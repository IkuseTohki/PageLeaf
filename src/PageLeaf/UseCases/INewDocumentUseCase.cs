namespace PageLeaf.UseCases
{
    /// <summary>
    /// 新規ドキュメントを作成し、エディタの状態を初期化するユースケースのインターフェースです。
    /// </summary>
    public interface INewDocumentUseCase
    {
        /// <summary>
        /// 新規作成を実行します。未保存の変更がある場合はユーザーに確認を行います。
        /// </summary>
        void Execute();
    }
}