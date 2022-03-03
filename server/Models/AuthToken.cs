using Microsoft.IdentityModel.Tokens;

namespace UserAuthServer.Models;

public class AuthToken {
    public SecurityToken SecurityToken { get; set; } = null!;

    public string Jwt { get; set; } = null!;
}
