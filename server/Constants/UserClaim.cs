public class UserClaim {
    private const string ClaimPrefix = "Claim_";
    private const string UserIdKey = "UserId";
    private const string UsernameKey = "Username";
    private const string RoleKey = "Role";

    public const string UserId = ClaimPrefix + UserIdKey;
    public const string Username = ClaimPrefix + UsernameKey;
    public const string Role = ClaimPrefix + RoleKey;
}
