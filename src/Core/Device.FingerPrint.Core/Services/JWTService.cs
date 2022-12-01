using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Device.FingerPrint.Core.Services
{
    public class JWTService
    {
        private readonly IAppSettings appSettings;
        public readonly byte[] keyJWT;

        public JWTService(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
            keyJWT = Encoding.ASCII.GetBytes(appSettings.CryptoPassword);
        }
        public string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", appSettings.ApiUrl) }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer = appSettings.ApiUrl,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyJWT), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
