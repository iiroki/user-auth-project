using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Utils;

namespace UserAuthServer.Services;

public class EmailConfirmationSender : IEmailConfirmService {
    private readonly ILogger<EmailConfirmationSender> Logger;
    private readonly string Server;
    private readonly int Port;
    private readonly string SenderName;
    private readonly string SenderEmail;
    private readonly string Password;
    private readonly string Host;

    public EmailConfirmationSender(
            ILogger<EmailConfirmationSender> logger,
            IConfiguration config) {
        this.Logger = logger;
        this.Server = config["Smtp:Server"];
        this.Port = Int32.Parse(config["Smtp:Port"]);
        this.SenderName = config["Smtp:SenderName"];
        this.SenderEmail = config["Smtp:SenderEmail"];
        this.Password = config["Smtp:Password"];
        this.Host = config["ServerUrl"];
    }

    public async Task SendConfirmationEmail(string apiEndpoint, User user, string confirmToken) {
        this.Logger.LogDebug($"{nameof(SendConfirmationEmail)} | Email: {user.Email}");
        var email = CreateDefaultMessage(user.Email);

        var bodyBuilder = new BodyBuilder();
        var messageBuilder = new StringBuilder();
        messageBuilder.Append(HtmlUtil.addHeading(1, "User Auth Project"));
        messageBuilder.Append($"Hello {user.Name}!");
        messageBuilder.Append("<br><br>");
        messageBuilder.Append("Confirm your email to be able to log in:");
        var confirmLink = HtmlUtil.addAnchor($"{this.Host}/{apiEndpoint}?userId={user.Id}&token={confirmToken}", "Confirm email");
        messageBuilder.Append(HtmlUtil.addHeading(2, confirmLink));
        bodyBuilder.HtmlBody = messageBuilder.ToString();
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        smtp.Connect(this.Server, this.Port, SecureSocketOptions.SslOnConnect);
        smtp.Authenticate(this.SenderEmail, this.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }

    private MimeMessage CreateDefaultMessage(string userEmail) {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(this.SenderEmail);
        email.To.Add(MailboxAddress.Parse(userEmail));
        email.Subject = "User Auth Project: Email confirmation";
        return email;
    }
}
