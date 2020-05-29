using LightApp.Contracts.Views;
using LightApp.ViewModels;

using MahApps.Metro.Controls;

namespace LightApp.Views
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
