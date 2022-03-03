using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UserAuthServer.Models;

// Summary: Web Shop user data model
public class User : IdentityUser {
    public string Name { get; set; } = null!;
}
