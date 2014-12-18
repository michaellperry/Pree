using Pree.ViewModels;
using System.Windows;
using UpdateControls.XAML;

namespace Pree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = ForView.Unwrap<MainViewModel>(DataContext);

            if (viewModel != null)
            {
                if (viewModel.CanClose)
                    viewModel.Closing();
                else
                    e.Cancel = true;
            }
        }
    }
}
