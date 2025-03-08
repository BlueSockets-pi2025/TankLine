using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationCode)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var smtpHost = smtpSettings["Host"] ?? throw new ArgumentNullException("SMTP Host is not configured");
        var smtpPort = int.TryParse(smtpSettings["Port"], out var port) ? port : throw new ArgumentNullException("SMTP Port is not configured or invalid");
        var smtpUsername = smtpSettings["Username"] ?? throw new ArgumentNullException("SMTP Username is not configured");
        var smtpPassword = smtpSettings["Password"] ?? throw new ArgumentNullException("SMTP Password is not configured");
        var senderEmail = smtpSettings["SenderEmail"] ?? throw new ArgumentNullException("Sender Email is not configured");

        using var smtpClient = new SmtpClient
        {
            Host = smtpHost,
            Port = smtpPort,
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword)
        };

        string htmlContent = $@"
        <html>
            <body style='font-family: Arial, sans-serif; background-color: #f7f9fc; color: #333; padding: 20px;'>
                <div style='text-align: center; padding: 20px;'>
                    <h1 style='color: #007bff; font-size: 32px; font-weight: 600;'>Welcome to TankLine</h1>
                    <p style='font-size: 18px; color: #555;'>You requested a login code to access TankLine, your epic multiplayer game.</p>
                    <p style='font-size: 22px; font-weight: bold; color: #007bff;'>Your login code is:</p>
                    <h2 style='color: #007bff; font-size: 48px; font-weight: bold;'>{verificationCode}</h2>
                    <p style='font-size: 16px; color: #555;'>This code is valid for 5 minutes. Please use it quickly.</p>
                    <p style='font-size: 14px; color: #888;'>If you did not request this code, please ignore this email.</p>
                </div>
                <div style='text-align: center; margin-top: 40px; font-size: 14px; color: #888;'>
                    <p>The Blue Socket Team - Creators of TankLine</p>
                    <p>TankLine | <a href='https://www.bluesocket.com' style='color: #007bff;'>bluesocket.com</a></p>
                </div>
            </body>
        </html>";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = "Your Login Code for TankLine",
            Body = htmlContent,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }
}
