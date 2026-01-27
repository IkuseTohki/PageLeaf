using Microsoft.Extensions.DependencyInjection;
using PageLeaf.ViewModels;
using PageLeaf.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PageLeaf.Services
{
    /// <summary>
    /// モードレスウィンドウの表示・管理を行うサービスの実装。
    /// </summary>
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Window> _openedWindows = new Dictionary<Type, Window>();

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// 指定した ViewModel に対応するウィンドウを表示します。
        /// </summary>
        /// <typeparam name="TViewModel">表示するウィンドウの ViewModel 型。</typeparam>
        public void ShowWindow<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModelType = typeof(TViewModel);

            // 既に開いている場合は最前面に表示して終了
            if (_openedWindows.TryGetValue(viewModelType, out var existingWindow))
            {
                if (existingWindow.WindowState == WindowState.Minimized)
                {
                    existingWindow.WindowState = WindowState.Normal;
                }
                existingWindow.Activate();
                return;
            }

            // ViewModel の解決
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            // Window の生成
            Window window = CreateWindowForViewModel(viewModel);
            if (window == null)
            {
                throw new InvalidOperationException($"No window mapping found for ViewModel: {viewModelType.Name}");
            }

            // Window の設定
            window.DataContext = viewModel;
            // メインウィンドウがある場合はオーナー設定（任意：タスクバーを分けるなら設定しない方が良い場合もあるが、アプリ内ツールとしては設定した方が自然な場合も）
            // モードレスツールウィンドウとして独立させるため、Ownerは設定しないでおく（仕様書の「独立して移動」はOwnerがあっても可能だが、最小化連動などをどうするかによる）
            // 一旦Owner設定なしで実装し、必要に応じて変更する。

            // 閉じたときのハンドリング
            window.Closed += (s, e) => _openedWindows.Remove(viewModelType);

            // 表示
            window.Show();
            _openedWindows.Add(viewModelType, window);
        }

        /// <summary>
        /// 指定した ViewModel に対応するウィンドウを閉じます。
        /// </summary>
        /// <typeparam name="TViewModel">閉じるウィンドウの ViewModel 型。</typeparam>
        public void CloseWindow<TViewModel>()
        {
            var viewModelType = typeof(TViewModel);
            if (_openedWindows.TryGetValue(viewModelType, out var window))
            {
                window.Close();
                // ClosedイベントハンドラでDictionaryから削除される
            }
        }

        /// <summary>
        /// 管理しているすべてのウィンドウを閉じます。
        /// </summary>
        public void CloseAllWindows()
        {
            // Close() を呼ぶと Closed イベントが発生し、_openedWindows から削除されるため、
            // コレクション変更例外を避けるためにリストのコピーを作成してループする。
            foreach (var window in new List<Window>(_openedWindows.Values))
            {
                window.Close();
            }
        }

        /// <summary>
        /// ViewModel に対応する Window を生成するヘルパーメソッド。
        /// マッピングロジックはここに集約する。
        /// </summary>
        private Window CreateWindowForViewModel(ViewModelBase viewModel)
        {
            return viewModel switch
            {
                CheatSheetViewModel _ => new CheatSheetWindow(),
                // 今後追加されるウィンドウはここに追加
                _ => throw new NotImplementedException($"ViewModel {viewModel.GetType().Name} is not mapped to any Window.")
            };
        }
    }
}
