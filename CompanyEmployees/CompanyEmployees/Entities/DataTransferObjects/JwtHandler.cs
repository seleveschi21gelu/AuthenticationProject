using CompanyEmployees.Entities.Models;
using CompanyEmployees.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CompanyEmployees.Entities.DataTransferObjects
{
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;
        private readonly UserManager<User> _userManager;
        private readonly RepositoryContext _dbContext;

        public JwtHandler(IConfiguration configuration, UserManager<User> userManager, RepositoryContext dbContext)
        {
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings");
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.GetSection("securityKey").Value);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public async Task<List<Claim>> GetClaims(User user)
        {
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, user.Email) };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSettings["validIssuer"],
                audience: _jwtSettings["validAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings["expiryInMinutes"])),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

        public string GenerateRefreshToken(User user)
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                user.RefreshToken = Convert.ToBase64String(randomNumber);
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                _dbContext.SaveChanges();
            }

            return user.RefreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings["securityKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
           
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public User GetUser(string username)
                => _dbContext.Users.SingleOrDefault(u => u.UserName == username);
    }
}
