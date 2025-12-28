
using PageLeaf.Models;
using System.Collections.Generic;

namespace PageLeaf.Services
{
    /// <summary>
    /// ファイルシステムへの読み書き操作を提供するサービスインターフェースです。
    /// Markdownファイルの開閉、保存、および汎用的なテキスト読み書きをサポートします。
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 指定されたパスからMarkdownファイルを読み込みます。
        /// </summary>
        /// <param name="filePath">読み込み対象のファイルパス。</param>
        /// <returns>読み込まれたコンテンツとメタデータを含むドキュメントオブジェクト。</returns>
        /// <exception cref="System.IO.FileNotFoundException">ファイルが見つからない場合にスローされます。</exception>
        MarkdownDocument Open(string filePath);

        /// <summary>
        /// 指定されたドキュメントをファイルシステムに保存します。
        /// </summary>
        /// <param name="document">保存するドキュメントオブジェクト。</param>
        /// <exception cref="System.ArgumentNullException">document が null の場合にスローされます。</exception>
        /// <exception cref="System.InvalidOperationException">FilePath が設定されていない場合にスローされます。</exception>
        void Save(MarkdownDocument document);

        /// <summary>
        /// 指定されたパスにファイルが存在するかどうかを確認します。
        /// </summary>
        /// <param name="filePath">確認するファイルパス。</param>
        /// <returns>存在する場合は true、それ以外は false。</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// 指定されたフォルダパスから、指定された検索パターンに一致するファイルのフルパスのリストを取得します。
        /// </summary>
        /// <param name="folderPath">検索対象のフォルダパス。</param>
        /// <param name="searchPattern">検索するファイルパターン（例: "*.css", "*.txt"）。</param>
        /// <returns>一致するファイルのフルパスのリスト。フォルダが存在しない場合やファイルが見つからない場合は空のリストを返します。</returns>
        IEnumerable<string> GetFiles(string folderPath, string searchPattern);

        /// <summary>
        /// 指定されたパスのテキストファイルをすべて読み込みます。
        /// </summary>
        /// <param name="filePath">読み込み対象のファイルパス。</param>
        /// <returns>ファイルの内容。</returns>
        string ReadAllText(string filePath);

        /// <summary>
        /// 指定された内容をテキストファイルとして書き込みます。既存のファイルがある場合は上書きされます。
        /// </summary>
        /// <param name="filePath">書き込み先のファイルパス。</param>
        /// <param name="content">書き込む内容。</param>
        void WriteAllText(string filePath, string content);
    }
}
