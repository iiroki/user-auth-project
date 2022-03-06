using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class UserUpdateDto : UserDeleteDto {
    [Required(ErrorMessage = "Name is required.")]
    public string? Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; } = null!;

    // Current password is in UserDeleteDto

    // Do not update password if null
    public string? NewPassword { get; set; } = null;
}
