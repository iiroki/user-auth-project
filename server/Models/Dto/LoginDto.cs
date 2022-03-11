using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class LoginDto {
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be 8-64 characters")]
    public string Password { get; set; } = null!;
}
