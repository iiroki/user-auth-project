using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UserAuthServer.Utils;

public class AuthSignKeyFactory {
    public static SymmetricSecurityKey CreateAuthSignKey(string secret) => (
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    );
}
