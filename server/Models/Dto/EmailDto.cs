using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class EmailDto {
    [Required(ErrorMessage = "Email is required.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;
}
