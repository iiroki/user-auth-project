using UserAuthServer.Models;

namespace UserAuthServer.Interfaces;

public interface IEmailConfirmService {
    Task SendConfirmationEmail(User user, string confirmToken);
}
