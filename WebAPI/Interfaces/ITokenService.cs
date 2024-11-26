using WebAPI.Entities;

namespace WebAPI.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
