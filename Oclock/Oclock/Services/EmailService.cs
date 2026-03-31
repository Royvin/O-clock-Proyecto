using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;

namespace Oclock.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Send(string toEmail, string subject, string htmlBody)
        {
            var smtpSection = _configuration.GetSection("Smtp");

            var host = smtpSection.GetValue<string>("Host") ?? "";
            var user = smtpSection.GetValue<string>("User") ?? "";
            var pass = smtpSection.GetValue<string>("Pass") ?? "";
            var from = smtpSection.GetValue<string>("From") ?? user;
            var port = smtpSection.GetValue<int>("Port");
            var enableSsl = smtpSection.GetValue<bool>("EnableSsl");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                throw new Exception("Configuración SMTP incompleta en appsettings.json (Smtp).");
            }

            using var message = new MailMessage();
            message.From = new MailAddress(from);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(host, port);
            smtp.EnableSsl = enableSsl;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(user, pass);

            smtp.Send(message);
        }
    }
}