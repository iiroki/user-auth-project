using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class UserCreateDto : LoginDto {
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;
}
