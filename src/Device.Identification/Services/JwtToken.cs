namespace Device.Identification.Services
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Security.Claims;
    using System.Text;

    public class JwtToken(IAppSettings appSettings)
    {
        private readonly IAppSettings _appSettings = appSettings;

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
