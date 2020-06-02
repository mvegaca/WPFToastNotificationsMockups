using System.Threading.Tasks;
using LightApp.Contracts.Activation;

namespace LightApp.Activation
{
    public class SampleActivationHandler : IActivationHandler
    {

        public SampleActivationHandler()
        {
        }

        public bool CanHandle()
            => false;

        public async Task HandleAsync()
        {
            await Task.CompletedTask;
        }
    }
}
