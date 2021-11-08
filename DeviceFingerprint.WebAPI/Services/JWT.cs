using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DeviceFingerprint.WebAPI.Services
{
    public class JWT
    {
        private readonly IAppSettings appSettings;

        public JWT(IAppSettings appSetting)
        {
            this.appSettings = appSetting;
        }
        public string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.EncryptPassword);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", appSettings.ApiUrl) }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer = appSettings.ApiUrl,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
