using Microsoft.AspNetCore.Identity;
using UserAuthServer.Models;

namespace UserAuthServer.Utils;

public class RequestUtil {
    public static async Task<User?> GetRequestUser(UserManager<User> userManager, HttpContext context) {
        return await userManager.GetUserAsync(context.User);
    }
}
