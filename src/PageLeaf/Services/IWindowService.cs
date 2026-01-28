using PageLeaf.ViewModels;

namespace PageLeaf.Services
{
    /// <summary>
    /// モードレスウィンドウの表示・管理を行うサービスインターフェース。
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// いずれかのサブウィンドウが閉じられたときに発生します。
        /// </summary>
        event System.EventHandler<System.Type> WindowClosed;

        /// <summary>
        /// 指定した ViewModel に対応するウィンドウを表示します。
        /// 既に開いている場合は、そのウィンドウを最前面に表示します。
        /// </summary>
        /// <typeparam name="TViewModel">表示するウィンドウの ViewModel 型。</typeparam>
        void ShowWindow<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// 指定した ViewModel に対応するウィンドウを閉じます。
        /// </summary>
        /// <typeparam name="TViewModel">閉じるウィンドウの ViewModel 型。</typeparam>
        void CloseWindow<TViewModel>();

        /// <summary>
        /// 管理しているすべてのウィンドウを閉じます。
        /// </summary>
        void CloseAllWindows();
    }
}
