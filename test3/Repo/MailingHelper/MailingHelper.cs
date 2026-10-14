using MimeKit;
using MailKit.Net.Smtp;

public interface IMailingHelper
{
    void SendMail(string to, string subject, string topic, string body, string name);
}

public class MailingHelper : IMailingHelper
{
    private readonly IConfiguration _configuration;

    public MailingHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendMail(string to, string subject, string topic, string body, string name)
    {
        var smtp = _configuration.GetSection("Smtp");
        var host = smtp["Host"];
        var senderEmail = smtp["SenderEmail"];
        var senderName = smtp["SenderName"] ?? "";
        var username = smtp["Username"];
        var password = smtp["Password"];
        var port = smtp.GetValue<int?>("Port") ?? 465;
        var useSsl = smtp.GetValue<bool?>("UseSsl") ?? true;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(senderEmail) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SMTP is not configured. Set Smtp:Host, Smtp:SenderEmail, Smtp:Username, and Smtp:Password before sending mail.");
        }

        var mailBody = File.ReadAllText("Repo/MailingHelper/mail.txt");
        var logoUrl = System.Net.WebUtility.HtmlEncode(
            test3.Configuration.PublicUrls.Create(_configuration, "mdc_mail.png").AbsoluteUri);
        body = mailBody.Replace("{{ content }}", body)
            .Replace("{{ topic }}", topic)
            .Replace("{{ logo_url }}", logoUrl);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(name, to));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            client.Connect(host, port, useSsl);
            client.Authenticate(username, password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
