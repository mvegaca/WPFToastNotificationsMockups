using System.Threading.Tasks;

using BasicApp.Core.Models;

namespace BasicApp.Core.Contracts.Services
{
    public interface IMicrosoftGraphService
    {
        Task<User> GetUserInfoAsync(string accessToken);

        Task<string> GetUserPhoto(string accessToken);
    }
}
