namespace UserAuthServer.Models;

public class AuthenticationToken {
    public string Token { get; set; } = null!;

    public DateTime Expires { get; set; }
}
