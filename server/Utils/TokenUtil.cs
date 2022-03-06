using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using UserAuthServer.Constants;
using UserAuthServer.Models;

namespace UserAuthServer.Utils;

public class TokenUtil {
    public static bool IsTokenType(JwtSecurityToken token, TokenType type) {
        var tokenType = token.Claims.Where(c => c.Type == TokenClaim.Type).FirstOrDefault();
        return tokenType != null && tokenType.Value == type.ToString();
    }

    public static async Task<User?> FindTokenUser(JwtSecurityToken token, UserManager<User> userManager) {
        var userIdClaim = token.Claims.Where(c => c.Type == TokenClaim.UserId).FirstOrDefault();
        return userIdClaim != null ? await userManager.FindByIdAsync(userIdClaim.Value) : null;
    }
}
