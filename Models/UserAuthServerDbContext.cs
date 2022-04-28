using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserAuthServer.Models;

public class UserAuthServerDbContext : IdentityDbContext<User, IdentityRole, string> {
    public UserAuthServerDbContext(DbContextOptions<UserAuthServerDbContext> options) : base(options) {
        // TODO
    }
}
