using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserAuthServer.Constants;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;

namespace UserAuthServer.Services;

public class JwtService : ITokenService {
    private readonly ILogger<JwtService> Logger;
    private readonly JwtSecurityTokenHandler TokenHandler = new JwtSecurityTokenHandler();
    private readonly TokenValidationParameters TokenOptions;

    public JwtService(
            ILogger<JwtService> logger,
            IOptions<TokenValidationParameters> tokenOptions) {
        this.Logger = logger;
        this.TokenOptions = tokenOptions.Value;
    }

    public AuthenticationToken CreateToken(TokenType type, string userId) {
        var authSignKey = this.TokenOptions.IssuerSigningKey;

        // [SECURE] Only add user ID and type claims
        var claims = new List<Claim> {
            new Claim(TokenClaim.Type, type.ToString()),
            new Claim(TokenClaim.UserId, userId)
        };

        // [SECURE] Add issuer and audience
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = CreateExpireDateTime(type),
            Issuer = this.TokenOptions.ValidIssuer,
            Audience = this.TokenOptions.ValidAudience,
            SigningCredentials = new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256)
        };

        var token = this.TokenHandler.CreateToken(tokenDescriptor);
        this.Logger.LogDebug($"{nameof(CreateToken)} | Created token with claims: {String.Join(", ", claims)}");

        return new AuthenticationToken {
            Expires = token.ValidTo,
            Token = this.TokenHandler.WriteToken(token)
        };
    }

    public JwtSecurityToken? ReadToken(string token) {
        try {
            this.TokenHandler.ValidateToken(token, this.TokenOptions, out var validToken);
            return validToken as JwtSecurityToken;
        } catch {
            return null;
        }
    }

    // [SECURE] Refresh and access tokens have different expiration time
    private DateTime CreateExpireDateTime(TokenType type) {
        switch (type) {
            case TokenType.Refresh:
                return DateTime.Now.AddHours(3);
            case TokenType.Access:
                return DateTime.Now.AddMinutes(5);
            default:
                throw new ArgumentException("Unknown token type");
        }
    }
}
