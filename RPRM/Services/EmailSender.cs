using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RPRM.Services
{

    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpClient client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com", //or another email sender provider
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("orange.rprm@gmail.com", "qcaxwmaedmtkeyaw")
            };

            return client.SendMailAsync("orange.rprm@gmail.com", email, subject, htmlMessage);
        }

    }
}
