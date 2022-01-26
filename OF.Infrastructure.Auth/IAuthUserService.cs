using OF.Auth.Core;
using System.Threading.Tasks;

namespace OF.Infrastructure.Auth.Services
{
    public interface IAuthUserService
    {
        Task<IAppUser> GetById(long userID);
        Task<IAppUser> GetById(string AppId, long userID);
        Task<IAppUser> GetByToken(string token);

    }
}
