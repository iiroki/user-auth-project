using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserAuthServer.Models;

public class UserAuthServerDbContext : IdentityDbContext<User, IdentityRole, string> {
    //public DbSet<Item> Items { get; set; } = default!;

    public UserAuthServerDbContext(DbContextOptions<UserAuthServerDbContext> options) : base(options) {
        this.Database.EnsureCreated();
    }
}
