using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models.Dto;

public class RoleUpdateDto {
    [Required(ErrorMessage = "Roles are required.")]
    public IList<string> Roles { get; set; } = null!;
}
