using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using MailKit;
namespace BLL.Services
{




    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Configuration;
    using MimeKit;

    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("System Name", _configuration["Email:SenderEmail"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            
            using var smtp = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput()));
            smtp.ServerCertificateValidationCallback = (s, c, ch, e) => true;
            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            
            smtp.Authenticate(_configuration["Email:Username"], _configuration["Email:Password"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}