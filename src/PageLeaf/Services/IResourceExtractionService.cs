namespace PageLeaf.Services
{
    /// <summary>
    /// アセンブリに埋め込まれたリソースを物理ファイルとして展開する機能を提供するサービスです。
    /// </summary>
    public interface IResourceExtractionService
    {
        /// <summary>
        /// 埋め込みリソースを適切な場所に展開します。
        /// </summary>
        /// <param name="baseDirectory">アプリケーションの実行ディレクトリ。</param>
        /// <param name="tempDirectory">アプリケーション用の一時ディレクトリ。</param>
        void ExtractAll(string baseDirectory, string tempDirectory);

        /// <summary>
        /// 指定された相対パスが、アプリケーション専用の内部リソース（一時フォルダ展開対象）かどうかを判定します。
        /// </summary>
        /// <param name="relativePath">リソースの相対パス（例: "css/extensions.css"）。</param>
        /// <returns>内部リソースであれば true。</returns>
        bool IsInternalResource(string relativePath);
    }
}
