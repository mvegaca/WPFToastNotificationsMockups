using System.Threading.Tasks;

using PrismApp.Core.Models;

namespace PrismApp.Core.Contracts.Services
{
    public interface IMicrosoftGraphService
    {
        Task<User> GetUserInfoAsync(string accessToken);

        Task<string> GetUserPhoto(string accessToken);
    }
}
