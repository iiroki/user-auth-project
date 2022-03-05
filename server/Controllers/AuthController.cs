using System.Security.Claims;
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
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LogIn(LoginDto login) {
        var user = await this.UserManager.FindByNameAsync(login.Username);
        if (user == null || !(await this.UserManager.CheckPasswordAsync(user, login.Password))) {
            return Unauthorized(ResponseUtil.CreateProblemDetails("Invalid login credentials"));
        }

        if (!user.EmailConfirmed) {
            return UnprocessableEntity(ResponseUtil.CreateProblemDetails("Email not confirmed"));
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

    [HttpPost("email-send-confirmation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendConfirmationEmail(EmailDto email) {
        this.Logger.LogDebug($"{nameof(SendConfirmationEmail)} | Email: {email.Email}");
        var user = await this.UserManager.FindByEmailAsync(email.Email);
        if (user != null) {
            var token = await this.UserManager.GenerateEmailConfirmationTokenAsync(user);
            Console.WriteLine("Email confirm token: " + token);
            // TODO
        }

        return NoContent();
    }

    [HttpPost("email-confirm")]
    public async Task<IActionResult> ConfirmEmail(EmailConfirmToken token) {
        this.Logger.LogDebug($"{nameof(ConfirmEmail)} | Token: {token.Token}");

        return NoContent();
    }
}
