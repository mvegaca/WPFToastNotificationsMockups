using MahApps.Metro.Controls;

using PrismApp.Contracts.Views;
using PrismApp.ViewModels;

namespace PrismApp.Views
{
    public partial class LogInWindow : MetroWindow, ILogInWindow
    {
        public LogInWindow(LogInViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();
    }
}
