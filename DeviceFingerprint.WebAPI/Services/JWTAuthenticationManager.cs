using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Services
{
    public class JWTAuthenticationManager : IJWTAuthenticationManager
    {
        private readonly string tokenKey;

        public JWTAuthenticationManager(string tokenKey)
        {
            this.tokenKey = tokenKey;
        }
        public string GenerateToken(int subscriberId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(tokenKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, subscriberId.ToString())
                }),
                //Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
