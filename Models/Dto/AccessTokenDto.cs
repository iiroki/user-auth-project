namespace UserAuthServer.Models.Dto;

public class AccessTokenDto : RefreshTokenDto {
    public string[] Roles { get; set; } = { };
}
