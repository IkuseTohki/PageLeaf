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
    }
}
