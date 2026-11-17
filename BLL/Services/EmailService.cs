
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class EmailService
    {
        //גישה להגדרות
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // =====================================
        // פונקציה מרכזית לשליחת מייל
        // =====================================
        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var senderEmail =
                _configuration["Email:SenderEmail"];

            var username =
                _configuration["Email:Username"];

            var password =
                _configuration["Email:Password"];

            if (string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "חסרות הגדרות לשליחת מייל."
                );
            }

            // הגדרות SMTP מתוך השרת
            var smtpServer =
                _configuration["Email:SmtpServer"]
                ?? "smtp.gmail.com";

            var portValue = _configuration["Email:Port"];

            int port = int.TryParse(
                portValue,
                out int configuredPort
            ) ? configuredPort : 465;

            // תמיכה בפורטים מאובטחים של Gmail
            if (port != 465 && port != 587)
            {
                throw new InvalidOperationException(
                    "SMTP port must be 465 or 587."
                );
            }

            var security = port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    "צוות תנופה",
                    senderEmail
                )
            );

            email.To.Add(
                MailboxAddress.Parse(toEmail)
            );

            email.Subject = subject;

            email.Body = new TextPart("plain")
            {
                Text = body
            };

            string stage = "Connect";

            try
            {
                using var smtp = new SmtpClient();

                // משתמשים באימות תעודת SSL הרגיל
                // אין לעקוף את בדיקות האבטחה

                _logger.LogInformation(
                    "SMTP connecting to {Server}:{Port}",
                    smtpServer,
                    port
                );

                await smtp.ConnectAsync(
                    smtpServer,
                    port,
                    security
                );

                _logger.LogInformation(
                    "SMTP connected successfully"
                );

                stage = "Authenticate";

                await smtp.AuthenticateAsync(
                    username,
                    password
                );

                _logger.LogInformation(
                    "SMTP authentication successful"
                );

                stage = "Send";

                await smtp.SendAsync(email);

                _logger.LogInformation(
                    "SMTP email sent successfully"
                );

                stage = "Disconnect";

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SMTP failed at stage {Stage}, " +
                    "server {Server}, port {Port}",
                    stage,
                    smtpServer,
                    port
                );

                throw;
            }
        }

        // =====================================
        // תאימות למנגנון ההתחברות הקיים
        // שומרת על הקריאות הישנות ל-SendEmail
        // =====================================
        public void SendEmail(
            string toEmail,
            string subject,
            string body)
        {
            SendEmailAsync(toEmail, subject, body)
                .GetAwaiter()
                .GetResult();
        }

        // =====================================
        // שליחת קוד אימות בהתחברות
        // =====================================
        public async Task SendLoginCodeAsync(
            string toEmail,
            string verificationCode)
        {
            string subject =
                "תנופה - קוד אימות להתחברות";

            string body = $@"שלום,

קיבלנו בקשת התחברות לחשבון שלך
במערכת תנופה.

קוד האימות שלך הוא:

{verificationCode}

אין להעביר את הקוד לאדם אחר.

אם לא ניסית להתחבר,
ניתן להתעלם מהודעה זו.

בברכה,
צוות תנופה";

            await SendEmailAsync(
                toEmail,
                subject,
                body
            );
        }

        // =====================================
        // שליחת קישור להגדרת סיסמה
        // לאחר אישור המנהל
        // =====================================
        public async Task SendPasswordSetupEmailAsync(
            string toEmail,
            string userName,
            string setupLink)
        {
            string subject =
                "תנופה - בקשת ההצטרפות שלך אושרה!";

            string body = $@"שלום {userName},

שמחים לעדכן שבקשת ההצטרפות שלך
למערכת תנופה אושרה בהצלחה!

כדי להשלים את פתיחת החשבון,
יש להגדיר סיסמה אישית באמצעות הקישור הבא:

{setupLink}

הקישור אישי ותקף למשך 24 שעות.

לאחר הגדרת הסיסמה ניתן להתחבר
למערכת תנופה באמצעות תעודת הזהות
והסיסמה שבחרת.

בברכה,
צוות תנופה";

            await SendEmailAsync(
                toEmail,
                subject,
                body
            );
        }
    }
}
