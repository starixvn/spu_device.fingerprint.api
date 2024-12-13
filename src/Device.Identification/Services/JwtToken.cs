namespace Device.Identification.Services
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public class JwtToken
    {
        private readonly IAppSettings _appSettings;

        public JwtToken(IAppSettings appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public SecurityTokenDescriptor GetSecurityTokenDescriptor()
        {
            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([new Claim("id", _appSettings.JwtID)]),
                Issuer = _appSettings.JwtID,
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.JwtSecretKey)),
                    SecurityAlgorithms.HmacSha256Signature)
            };
        }
    }

}
