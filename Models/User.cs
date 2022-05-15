using Microsoft.AspNetCore.Identity;

namespace UserAuthServer.Models;

public class User : IdentityUser {
    public string Name { get; set; } = null!;
    public IList<UserFile> Files { get; set; } = null!;
}
