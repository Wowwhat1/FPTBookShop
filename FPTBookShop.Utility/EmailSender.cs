using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTBookShop.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridScret { get; set; }

        public EmailSender(IConfiguration _config)
        {
            SendGridScret = _config.GetValue<string>("SendGrid: SecretKey");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // logic to send email
            var client = new SendGridClient(SendGridScret);

            var from = new EmailAddress("hello@dotnetmastery.com", "Bulk Book");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            return client.SendEmailAsync(message);
        }
    }
}
