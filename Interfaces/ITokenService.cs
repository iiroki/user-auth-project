using System.IdentityModel.Tokens.Jwt;
using UserAuthServer.Constants;
using UserAuthServer.Models;

namespace UserAuthServer.Interfaces;

public interface ITokenService {
    AuthenticationToken CreateToken(TokenType type, string userId);

    JwtSecurityToken? ReadToken(string token);
}
