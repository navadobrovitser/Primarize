
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
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

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback =
    (s, c, ch, e) => true;

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                username,
                password
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
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
