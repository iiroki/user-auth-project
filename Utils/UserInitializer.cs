using Microsoft.AspNetCore.Identity;
using UserAuthServer.Models;
using UserAuthServer.Constants;

namespace UserAuthServer.Initialization;

public class UserInitializer {
    public static async Task seedUserRoles(RoleManager<IdentityRole> roleManager) {
        if (await roleManager.FindByNameAsync(UserRole.Admin) == null) {
            await roleManager.CreateAsync(new IdentityRole { Name = UserRole.Admin });
        }

        if (await roleManager.FindByNameAsync(UserRole.User) == null) {
            await roleManager.CreateAsync(new IdentityRole { Name = UserRole.User });
        }
    }

    public static async Task seedUsers(UserManager<User> userManager, IConfiguration config) {
        var existing = await userManager.FindByNameAsync(config["PowerUser:Username"]);
        if (existing == null) {
            var powerUser = new User {
                UserName = config["PowerUser:Username"],
                Name = config["PowerUser:Name"],
                Email = config["PowerUser:Email"],
                EmailConfirmed = true
            };

            await userManager.CreateAsync(powerUser, config["PowerUser:Password"]);
            await userManager.AddToRolesAsync(powerUser, new List<string>{ UserRole.User, UserRole.Admin });
        }
    }
}
