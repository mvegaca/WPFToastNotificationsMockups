using BasicApp.Contracts.Views;
using BasicApp.ViewModels;

using MahApps.Metro.Controls;

namespace BasicApp.Views
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
