namespace UserAuthServer.Constants;

public class UserRole {
    public const string Admin = "admin";
    public const string User = "user";

    public static IList<string> GetAll() => new List<string> {UserRole.Admin, UserRole.User};
}
