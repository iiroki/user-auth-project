using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class UserDeleteDto {
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = null!;
}
