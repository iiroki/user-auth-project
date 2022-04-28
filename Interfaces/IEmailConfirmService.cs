using UserAuthServer.Models;

namespace UserAuthServer.Interfaces;

public interface IEmailConfirmService {
    Task SendConfirmationEmail(string apiEndpoint, User user, string confirmToken);
}
