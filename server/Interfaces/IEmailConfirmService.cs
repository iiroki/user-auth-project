namespace UserAuthServer.Interfaces;

public interface IEmailConfirmService {
    void sendConfirmationEmail(string confirmToken);
}
