
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

namespace PageLeaf.Services
{
    /// <summary>
    /// アプリケーション設定の読み込みと保存を管理するサービスインターフェースです。
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 設定が変更されたときに発生するイベント。
        /// </summary>
        event System.EventHandler<ApplicationSettings>? SettingsChanged;

        /// <summary>
        /// 現在メモリ上にある設定オブジェクトを取得します。
        /// </summary>
        ApplicationSettings CurrentSettings { get; }

        /// <summary>
        /// 設定をファイルから読み込みます。ファイルが存在しない場合は、デフォルト設定を生成して返します。
        /// </summary>
        /// <returns>読み込まれた設定オブジェクト。</returns>
        ApplicationSettings LoadSettings();

        /// <summary>
        /// 指定された設定をファイルに保存します。
        /// </summary>
        /// <param name="settings">保存する設定オブジェクト。</param>
        /// <exception cref="System.ArgumentNullException">settings が null の場合にスローされます。</exception>
        void SaveSettings(ApplicationSettings settings);
    }
}
