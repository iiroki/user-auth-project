using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace UserAuthServer.Services;

public class BCryptPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class {
    private static readonly int WORK_FACTOR = 12; // [SECURE] OWASP: Minimum work factor = 10
    private readonly ILogger<BCryptPasswordHasher<TUser>> Logger;

    public BCryptPasswordHasher(ILogger<BCryptPasswordHasher<TUser>> logger){
        this.Logger = logger;
    }

    public string HashPassword(TUser user, string password) {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // [SECURE] Let BCrypt generate salt
        var hash = BCrypt.Net.BCrypt.HashPassword(password, WORK_FACTOR);
        this.Logger.LogDebug($"{nameof(HashPassword)} | Took: {stopwatch.ElapsedMilliseconds} ms");
        return hash;
    }

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword) {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var valid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        if (valid && BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WORK_FACTOR)) {
            return PasswordVerificationResult.SuccessRehashNeeded;
        }

        this.Logger.LogDebug($"{nameof(VerifyHashedPassword)} | Took: {stopwatch.ElapsedMilliseconds} ms");
        return valid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}
