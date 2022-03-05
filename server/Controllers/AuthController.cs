using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Models.Dto;
using UserAuthServer.Utils;

namespace UserAuthServer.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase {
    private readonly ILogger<AuthController> Logger;
    private readonly UserManager<User> UserManager;
    private readonly IdentityOptions IdentityOptions;
    private readonly ITokenService TokenService;

    public AuthController(
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            IOptions<IdentityOptions> options,
            ITokenService tokenService) {
        this.Logger = logger;
        this.UserManager = userManager;
        this.IdentityOptions = options.Value;
        this.TokenService = tokenService;
    }

    /// <summary>
    ///     Log in
    /// </summary>
    /// <remarks>
    ///     Produces a JWT that can be used to authenticate an user.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogIn(LoginDto login) {
        var user = await this.UserManager.FindByNameAsync(login.Username);
        if (user == null || !(await this.UserManager.CheckPasswordAsync(user, login.Password))) {
            return Unauthorized(ResponseUtil.CreateProblemDetails("Invalid login credentials"));
        }

        // Add identity claim
        var claims = new List<Claim> {
            new Claim(this.IdentityOptions.ClaimsIdentity.UserIdClaimType, user.Id)
        };

        // Add role claims
        var userRoles = await this.UserManager.GetRolesAsync(user);
        foreach (var role in userRoles) {
            claims.Add(new Claim(this.IdentityOptions.ClaimsIdentity.RoleClaimType, role));
        }

        var token = this.TokenService.createToken(claims);
        this.Logger.LogDebug($"{nameof(LogIn)} | ID: {user.Id} - Username: {user.UserName}");

        return Ok(new AuthTokenDto {
            Token = token.Jwt,
            Expires = token.SecurityToken.ValidTo,
            Roles = userRoles.ToArray()
        });
    }
}
