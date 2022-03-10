namespace UserAuthServer.Utils;

public class HtmlUtil {
    public static string addHeading(uint level, string text) {
        return $"<h{level}>{text}<h{level}>";
    }

    public static string addAnchor(string href, string text) {
        return $"<a href={href}>{text}</a>";
    }
}
