namespace UserAuthServer.Models.Dto;

public class AuthTokenDto {
    public string Token { get; set; } = null!;

    public DateTime Expires { get; set; }

    public string[] Roles { get; set; } = {};
}
