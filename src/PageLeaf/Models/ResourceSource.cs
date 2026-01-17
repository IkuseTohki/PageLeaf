namespace PageLeaf.Models
{
    /// <summary>
    /// ライブラリ（Highlight.js, Mermaid）の読み込み先を示します。
    /// </summary>
    public enum ResourceSource
    {
        /// <summary>
        /// アプリケーション同梱のローカルファイルを使用します。
        /// </summary>
        Local,

        /// <summary>
        /// 外部の CDN (unpkg.com 等) を使用します。
        /// </summary>
        Cdn
    }
}
