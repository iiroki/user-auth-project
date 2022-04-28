using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class RefreshDto {
    [Required(ErrorMessage = "Refresh token is required.")]
    public string Token { get; set; } = null!;
}
