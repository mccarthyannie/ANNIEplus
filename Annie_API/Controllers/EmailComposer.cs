using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace Annie_API.Controllers
{
    public class EmailComposer : IEmailComposer
    {
        private readonly IConfiguration _configuration;

        public EmailComposer(IConfiguration configuration) 
        {
            _configuration = configuration;
        }
        
        public bool ComposeEmail(string recipientName, string recipientEmail, string subject, string body)
        {
            try
            {
                var from = _configuration["Mail:From"];
                var name = _configuration["Mail:Name"];
                var smtp = _configuration["Mail:Smtp"];
                var port = _configuration["Mail:Port"];
                var password = _configuration["Mail:Password"];

                var message = new MimeMessage(); 
                message.From.Add(new MailboxAddress(name, from!));
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                message.Subject = subject;

                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port!), false);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                // represents the client that will send the email
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("### Error with EmailComposer: ###");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
