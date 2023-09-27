using CatalogoApi.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace CatalogoApi.Services
{
    public class TokenService : ITokenService
    {
        public string GetToken(string key, string issuer, string audience, UserModel user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            // gerar chave segura
            var sercurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // gerar chave simetrica
            var credentials = new SigningCredentials(sercurityKey,
                SecurityAlgorithms.HmacSha256);

            // gerar token
            var token = new JwtSecurityToken(issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: credentials);

            // desserializar token para retornar uma string
            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }
    }
}

