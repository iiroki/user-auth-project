using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserAuthServer.Constants;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Models.Dto;
using UserAuthServer.Utils;

namespace UserAuthServer.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController : ControllerBase {
    private readonly ILogger<AuthController> Logger;
    private readonly UserManager<User> UserManager;
    private readonly IdentityOptions IdentityOptions;
    private readonly ITokenService TokenService;
    private readonly IEmailConfirmService EmailConfirmService;

    public AuthController(
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            IOptions<IdentityOptions> options,
            ITokenService tokenService,
            IEmailConfirmService emailConfirmService) {
        this.Logger = logger;
        this.UserManager = userManager;
        this.IdentityOptions = options.Value;
        this.TokenService = tokenService;
        this.EmailConfirmService = emailConfirmService;
    }

    /// <summary>
    ///     Log in
    /// </summary>
    /// <remarks>
    ///     Produces a refresh token that can be used to gain access token.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RefreshTokenDto), StatusCodes.Status200OK)]
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

        var token = this.TokenService.CreateToken(TokenType.Refresh, user.Id);
        this.Logger.LogDebug($"{nameof(LogIn)} | ID: {user.Id}, Username: {user.UserName}");

        return Ok(new RefreshTokenDto {
            Token = token.Token,
            Expires = token.Expires
        });
    }

    /// <summary>
    ///     Refresh
    /// </summary>
    /// <remarks>
    ///     Produces an access token based on the refresh token.
    /// </remarks>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AccessTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refresh(RefreshDto refresh) {
        var refreshToken = this.TokenService.ReadToken(refresh.Token);
        if (refreshToken == null || !TokenUtil.IsTokenType(refreshToken, TokenType.Refresh)) {
            return BadRequest(ResponseUtil.CreateProblemDetails("Expected refresh token"));
        }

        var user = await TokenUtil.FindTokenUser(refreshToken, this.UserManager);
        if (user == null) {
            return NotFound(ResponseUtil.CreateProblemDetails("Token user not found"));
        }

        var roles = await this.UserManager.GetRolesAsync(user);
        var token = this.TokenService.CreateToken(TokenType.Access, user.Id);
        return Ok(new AccessTokenDto {
            Token = token.Token,
            Expires = token.Expires,
            Roles = roles.ToArray()
        });
    }

    /// <summary>
    ///     Send email confirmation
    /// </summary>
    /// <remarks>
    ///     Sends an email confirmation message to the user's email if the user is found and the email is not already confirmed.
    /// </remarks>
    [HttpPost("email-send-confirmation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendConfirmationEmail(EmailDto email) {
        var user = await this.UserManager.FindByEmailAsync(email.Email);
        this.Logger.LogDebug($"{nameof(SendConfirmationEmail)} | Email: {email.Email}, Found: {user != null}");
        if (user != null && !user.EmailConfirmed) {
            var token = await this.UserManager.GenerateEmailConfirmationTokenAsync(user);
            await this.EmailConfirmService.SendConfirmationEmail("auth/email-confirm", user, token);
        }

        return NoContent();
    }

    /// <summary>
    ///     Confirm email
    /// </summary>
    /// <remarks>
    ///     Confirms user's email address if the token matches the user ID.
    /// </remarks>
    [HttpGet("email-confirm")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail(
            [FromQuery(Name = "userId")] string userId,
            [FromQuery(Name = "token")] string token) {
        var encodedToken = token.Replace(' ', '+');
        this.Logger.LogDebug($"{nameof(ConfirmEmail)} | User ID: {userId}, Token: {encodedToken}");
        var user = await this.UserManager.FindByIdAsync(userId);
        if (user == null) {
            return NotFound("User not found");
        } else if (user.EmailConfirmed) {
            return BadRequest("Email already confirmed");
        }

        var result = await this.UserManager.ConfirmEmailAsync(user, encodedToken);
        if (!result.Succeeded) {
            return BadRequest("Email confirmation failed");
        }

        return Ok("Email confirmed successfully");
    }
}
