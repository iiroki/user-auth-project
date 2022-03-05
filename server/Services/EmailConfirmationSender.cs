using UserAuthServer.Interfaces;
using UserAuthServer.Models;

namespace UserAuthServer.Services;

public class EmailConfirmationSender : IEmailConfirmService {
    private readonly ILogger<EmailConfirmationSender> Logger;
    private readonly string Server;
    private readonly int Port;
    private readonly string SenderName;
    private readonly string SenderEmail;
    private readonly string Password;

    public EmailConfirmationSender(
            ILogger<EmailConfirmationSender> logger,
            IConfiguration config) {
        this.Logger = logger;
        this.Server = config["Smtp:Server"];
        this.Port = Int32.Parse(config["Smtp:Port"]);
        this.SenderName = config["Smtp:SenderName"];
        this.SenderEmail = config["Smtp:SenderEmail"];
        this.Password = config["Smtp:Password"];
    }

    public async Task SendConfirmationEmail(User user, string confirmToken) {
        this.Logger.LogDebug($"{nameof(SendConfirmationEmail)} | Email: {user.Email}");
        // TODO
    }
}