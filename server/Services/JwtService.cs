using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Utils;

namespace UserAuthServer.Services;

public class JwtService : ITokenService {
    private readonly ILogger<JwtService> Logger;
    private readonly string JwtSecret;
    private readonly JwtSecurityTokenHandler TokenHandler = new JwtSecurityTokenHandler();

    public JwtService(
            ILogger<JwtService> logger,
            IConfiguration config) {
        this.Logger = logger;
        this.JwtSecret = config["Jwt:Secret"];
    }

    public AuthToken createToken(IList<Claim> claims) {
        var authSignKey = AuthSignKeyFactory.CreateAuthSignKey(this.JwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddHours(2),
            SigningCredentials = new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = this.TokenHandler.CreateToken(tokenDescriptor);
        this.Logger.LogDebug($"{nameof(createToken)} | Created token with claims: {String.Join(", ", claims)}");

        return new AuthToken {
            SecurityToken = token,
            Jwt = this.TokenHandler.WriteToken(token)
        };
    }
}
