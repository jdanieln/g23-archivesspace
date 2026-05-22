using System.Threading.Tasks;
using ArchivesSpaceWeb.Domains.Identity.Entities;

namespace ArchivesSpaceWeb.Domains.Identity.Interfaces
{
    public interface IAuthService
    {
        Task<User?> ValidateUserAsync(string username, string password, string authMode);
    }
}
