using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using WebAPI.Entities;
using WebAPI.Interfaces;

namespace WebAPI.Services
{
    public class TokenService : ITokenService
    {

        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config, UserManager<AppUser> userManager) 
        {
            _config = config;
            _userManager = userManager;
        }
        public async Task<string> CreateToken(AppUser user)
        {
            var tokenKey = _config["TokenKey"] ?? throw new Exception("Cannot access tokenKy from appSettings");

            if (tokenKey.Length < 64)
                throw new Exception("Your tokenKey needs to be longer");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

            if (user.UserName == null) throw new Exception("No username for user");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
