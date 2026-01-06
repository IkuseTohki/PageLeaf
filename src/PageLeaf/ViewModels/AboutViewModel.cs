using System.Reflection;
using System.Windows.Input;
using PageLeaf.Utilities;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// バージョン情報画面のロジックを保持するViewModelです。
    /// </summary>
    public class AboutViewModel : ViewModelBase
    {
        public string AppName { get; }
        public string Version { get; }
        public string Copyright { get; }
        public string Description { get; }

        public ICommand CloseCommand { get; }

        public AboutViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();

            AppName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "PageLeaf";
            Version = $"バージョン: {assembly.GetName().Version?.ToString() ?? "1.0.0"}";
            Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright © 2026 PageLeaf Project";
            Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "Markdown CSS エディター & ビューアー";

            CloseCommand = new DelegateCommand(ExecuteClose);
        }

        private void ExecuteClose(object? parameter)
        {
            if (parameter is System.Windows.Window window)
            {
                window.Close();
            }
        }
    }
}
