
using PageLeaf.Models;
using System.Collections.Generic;

namespace PageLeaf.Services
{
    public interface IFileService
    {
        MarkdownDocument Open(string filePath);
        void Save(MarkdownDocument document);
        bool FileExists(string filePath);
        /// <summary>
        /// 指定されたフォルダパスから、指定された検索パターンに一致するファイルのフルパスのリストを取得します。
        /// </summary>
        /// <param name="folderPath">検索対象のフォルダパス。</param>
        /// <param name="searchPattern">検索するファイルパターン（例: "*.css", "*.txt"）。</param>
        /// <returns>一致するファイルのフルパスのリスト。フォルダが存在しない場合やファイルが見つからない場合は空のリストを返します。</returns>
        IEnumerable<string> GetFiles(string folderPath, string searchPattern);
    }
}
