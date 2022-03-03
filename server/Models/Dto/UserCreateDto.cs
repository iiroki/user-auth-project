using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class UserCreateDto : LoginDto {
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = null!;
}
