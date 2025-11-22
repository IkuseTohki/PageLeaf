using PageLeaf.ViewModels;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace PageLeaf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Currently, there's a compilation issue with subscribing to CoreWebView2Initialized directly.
            // A more robust solution for ViewModel notification will be explored later if needed.
            // For now, WebBrowserHelper implicitly handles EnsureCoreWebView2Async.
        }
    }
}
