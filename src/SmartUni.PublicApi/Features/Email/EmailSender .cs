using System.Net;
using System.Net.Mail;
using SmartUni.PublicApi.Features.Email.Interface;

namespace SmartUni.PublicApi.Features.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            string fromEmail = _configuration["Email:Gmail:From"];
            string appPassword = _configuration["Email:Gmail:AppPassword"];

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "SmartUni Team"),
                Subject = subject,
                Body = message,
                IsBodyHtml = false
            };

            mailMessage.To.Add(email);

            try
            {
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw; // optional: rethrow or handle depending on use case
            }
        }
    }
}
