using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace Annie_API.Controllers
{
    public interface IEmailComposer
    {
        bool ComposeEmail(string recipientName, string recipientEmail, string subject, string body);
    }
}
