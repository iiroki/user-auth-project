using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class UserCreateDto {
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be 8-64 characters")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; } = null!;
}
