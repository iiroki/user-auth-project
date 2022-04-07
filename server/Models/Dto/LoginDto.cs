using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class LoginDto {
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
}
