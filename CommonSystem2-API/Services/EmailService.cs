using System.Net;
using System.Net.Mail;

namespace CommonSystem2_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string recipient, string subject, string body)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                string _smtpServer = _configuration["SMTPSettings:SMTPServer"]; 
                int _port = Convert.ToInt32(_configuration["SMTPSettings:SMTPPort"]);
                string _senderEmail = _configuration["SMTPSettings:SenderEmail"];
                string _senderPassword = _configuration["SMTPSettings:SendPassword"];
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_senderEmail);
                mail.To.Add(recipient);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true; 
                using (SmtpClient smtpClient = new SmtpClient(_smtpServer, _port))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    await smtpClient.SendMailAsync(mail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                return false;
            }
        }
    }
}
