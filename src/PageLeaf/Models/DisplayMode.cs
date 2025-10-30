namespace PageLeaf.Models
{
    /// <summary>
    /// エディタの表示モードを定義します。
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// 閲覧専用モード。
        /// </summary>
        Viewer,

        /// <summary>
        /// Markdown テキストの編集モード。
        /// </summary>
        Markdown,

        /// <summary>
        /// リアルタイム編集モード。
        /// </summary>
        RealTime
    }
}
