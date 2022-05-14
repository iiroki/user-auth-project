using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserAuthServer.Models;

public class UserAuthServerDbContext : IdentityDbContext<User, IdentityRole, string> {
    public DbSet<UserFile> UserFiles { get; set; } = null!;

    public UserAuthServerDbContext(DbContextOptions<UserAuthServerDbContext> options) : base(options) {
        // NOP
    }
}
