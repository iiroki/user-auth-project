using Microsoft.AspNetCore.Identity;

namespace UserAuthServer.Services;

public class BCryptPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class {
    private static readonly int WORK_FACTOR = 16;

    public string HashPassword(TUser user, string password) {
        // Let BCrypt generate salt
        return BCrypt.Net.BCrypt.HashPassword(password, WORK_FACTOR);
    }

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword) {
        var valid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        if (valid && BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WORK_FACTOR)) {
            return PasswordVerificationResult.SuccessRehashNeeded;
        }

        return valid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}
