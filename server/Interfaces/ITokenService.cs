using System.Security.Claims;
using UserAuthServer.Models;

namespace UserAuthServer.Interfaces;

public interface ITokenService {
    AuthToken createToken(IList<Claim> claims);
}
