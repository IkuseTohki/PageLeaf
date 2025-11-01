using System;
using System.Windows.Input;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// デリゲートを受け取る ICommand の実装です。
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// コマンドの実行可能状態が変更されたときに発生します。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 新しいコマンドを作成します。
        /// </summary>
        /// <param name="execute">コマンドの実行ロジック。</param>
        /// <param name="canExecute">コマンドの実行可能状態を判断するロジック。</param>
        public DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 現在の状態でコマンドが実行可能かどうかを判断します。
        /// </summary>
        /// <param name="parameter">コマンドのパラメータ。</param>
        /// <returns>コマンドが実行可能な場合は true、それ以外は false。</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// コマンドを実行します。
        /// </summary>
        /// <param name="parameter">コマンドのパラメータ。</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
