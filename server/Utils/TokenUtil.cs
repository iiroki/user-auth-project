using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UserAuthServer.Constants;
using UserAuthServer.Models;

namespace UserAuthServer.Utils;

public class TokenUtil {
    public static bool HasTokenTypeClaim(IEnumerable<Claim> claims, TokenType type) {
        var typeClaim = claims.Where(c => c.Type == TokenClaim.Type).FirstOrDefault();
        return typeClaim != null && typeClaim.Value == type.ToString();
    }

    public static bool IsTokenType(JwtSecurityToken token, TokenType type) {
        return HasTokenTypeClaim(token.Claims, type);
    }

    public static async Task<User?> FindClaimUser(IEnumerable<Claim> claims, UserManager<User> userManager) {
        var userIdClaim = claims.Where(c => c.Type == TokenClaim.UserId).FirstOrDefault();
        return userIdClaim != null ? await userManager.FindByIdAsync(userIdClaim.Value) : null;

    }

    public static async Task<User?> FindTokenUser(JwtSecurityToken token, UserManager<User> userManager) {
        return await FindClaimUser(token.Claims, userManager);
    }
}
