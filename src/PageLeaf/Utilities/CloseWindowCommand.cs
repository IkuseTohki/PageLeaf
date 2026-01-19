using System;
using System.Windows;
using System.Windows.Input;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// パラメータとして渡された Window を閉じるコマンド。
    /// </summary>
    public class CloseWindowCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}
