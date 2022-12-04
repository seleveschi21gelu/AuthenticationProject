using CompanyEmployees.Entities.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CompanyEmployees.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(SigningCredentials signingCredentials, List<Claim> claims);
        string GenerateRefreshToken(User user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        User GetUser(string username);
        bool RevokeRefreshToken(string username);
    }
}
