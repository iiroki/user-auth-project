using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UserAuthServer.Constants;
using UserAuthServer.Models;
using UserAuthServer.Utils;

namespace UserAuthServer.Middleware;

public class UserRoleMiddleware : IMiddleware {
    private readonly ILogger<UserRoleMiddleware> Logger;
    private readonly UserManager<User> UserManager;

    public UserRoleMiddleware(
            ILogger<UserRoleMiddleware> logger,
            UserManager<User> userManager) {
        this.Logger = logger;
        this.UserManager = userManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        var claims = context.User.Claims;

        // [SECURE] Only add role claims when access token is used
        if (TokenUtil.HasTokenTypeClaim(claims, TokenType.Access) && claims.Count() > 0) {
            var user = await TokenUtil.FindClaimUser(claims, this.UserManager);
            if (user != null) {
                var roles = await this.UserManager.GetRolesAsync(user);
                var roleClaims = new List<Claim>();
                foreach (var role in roles) {
                    roleClaims.Add(new Claim(TokenClaim.Role, role));
                }

                context.User.AddIdentity(new ClaimsIdentity(roleClaims, null, TokenClaim.Username, TokenClaim.Role));
                this.Logger.LogDebug($"UserRoleMiddleware | User: {user}, Role claims: {String.Join(", ", roleClaims)}");
            }
        }

        await next(context);
    }
}
