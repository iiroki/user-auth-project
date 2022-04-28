public class TokenClaim {
    private const string ClaimPrefix = "Claim_";

    private const string UserIdKey = "UserId";
    private const string UsernameKey = "Username";
    private const string RoleKey = "UserRole";
    private const string TypeKey = "Type";

    public const string UserId = ClaimPrefix + UserIdKey;
    public const string Username = ClaimPrefix + UsernameKey;
    public const string Role = ClaimPrefix + RoleKey;
    public const string Type = ClaimPrefix + TypeKey;
}
