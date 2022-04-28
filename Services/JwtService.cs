using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using UserAuthServer.Constants;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Utils;

namespace UserAuthServer.Services;

public class JwtService : ITokenService {
    private readonly ILogger<JwtService> Logger;
    private readonly string JwtSecret;
    private readonly JwtSecurityTokenHandler TokenHandler = new JwtSecurityTokenHandler();
    private readonly TokenValidationParameters TokenValidationParameters;

    public JwtService(
            ILogger<JwtService> logger,
            IConfiguration config) {
        this.Logger = logger;
        this.JwtSecret = config["Jwt:Secret"];
        // TODO Fetch these from the config!
        this.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = AuthSignKeyFactory.CreateAuthSignKey(this.JwtSecret),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    }

    public AuthenticationToken CreateToken(TokenType type, string userId) {
        var authSignKey = AuthSignKeyFactory.CreateAuthSignKey(this.JwtSecret);
        var claims = new List<Claim> {
            new Claim(TokenClaim.Type, type.ToString()),
            new Claim(TokenClaim.UserId, userId)
        };

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = CreateExpireDateTime(type),
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
            this.TokenHandler.ValidateToken(token, this.TokenValidationParameters, out var validToken);
            return validToken as JwtSecurityToken;
        } catch {
            return null;
        }
    }

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
