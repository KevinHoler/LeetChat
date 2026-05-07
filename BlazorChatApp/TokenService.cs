using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using BlazorChatApp.Data.Entities;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace BlazorChatApp
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration) =>
            new()
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                IssuerSigningKey = GetSecurityKey(configuration),
            };
        
        public string GenerateJWT(IEnumerable<Claim> additionalClaims = null)
        {
            var securityKey = GetSecurityKey(_configuration);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expireInMinutes = Convert.ToInt32(_configuration["Jwt:ExpireInMinutes"] ?? "60");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (additionalClaims?.Any() == true)
                claims.AddRange(additionalClaims);

            var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],
                audience: "*",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateJWT(User user, IEnumerable<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                
            };
            if (additionalClaims?.Any() == true)
                claims.AddRange(additionalClaims);

            return GenerateJWT(claims);
        }

        private static SymmetricSecurityKey GetSecurityKey(IConfiguration _configuration) =>
            new(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    }
}
