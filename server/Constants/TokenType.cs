using System.ComponentModel;

namespace UserAuthServer.Constants;

public enum TokenType {
    [Description("refresh")]
    Refresh,

    [Description("access")]
    Access
}
