using System.Windows;
using System.Windows.Input;
using PageLeaf.Utilities;

namespace PageLeaf.ViewModels
{
    public class InputViewModel : ViewModelBase
    {
        private string _inputText = string.Empty;
        private string _message = string.Empty;
        private string _title = string.Empty;

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public bool? DialogResult { get; private set; }

        public InputViewModel()
        {
            OkCommand = new DelegateCommand(ExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        private void ExecuteOk(object? parameter)
        {
            DialogResult = true;
            CloseWindow(parameter);
        }

        private void ExecuteCancel(object? parameter)
        {
            DialogResult = false;
            CloseWindow(parameter);
        }

        private void CloseWindow(object? parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = DialogResult;
                window.Close();
            }
        }
    }
}
