using System.Diagnostics;

using PrismApp.Contracts.Services;

namespace PrismApp.Services
{
    public class SystemService : ISystemService
    {
        public SystemService()
        {
        }

        public void OpenInWebBrowser(string url)
        {
            // For more info see https://github.com/dotnet/corefx/issues/10361
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
