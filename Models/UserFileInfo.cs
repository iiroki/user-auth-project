using System.ComponentModel.DataAnnotations;

namespace UserAuthServer.Models;

public class UserFileInfo {
    public string Id { get; set; } = null!;
    public User User { get; set; } = null!;
}
