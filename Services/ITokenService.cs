using CatalogoApi.Models;

namespace CatalogoApi.Services
{
    public interface ITokenService
    {
        string GetToken(string key, string issuer, string audience, UserModel user);
    }
}
