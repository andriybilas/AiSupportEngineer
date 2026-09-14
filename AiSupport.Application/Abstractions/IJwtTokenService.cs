using AiSupport.Application.Models;

namespace AiSupport.Application.Abstractions
{
    public interface IJwtTokenService
    {
        JwtTokenResult CreateToken(Guid userId, string userName);
    }
}
