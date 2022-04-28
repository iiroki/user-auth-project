using Microsoft.AspNetCore.Mvc;

namespace UserAuthServer.Utils;

public class ResponseUtil {
    public static ProblemDetails CreateProblemDetails(string title) => new ProblemDetails {
        Title = title
    };
}
