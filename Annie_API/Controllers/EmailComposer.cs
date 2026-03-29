using MimeKit;

namespace Annie_API.Controllers
{
    public class EmailComposer
    {
        public async Task<bool>  ComposeEmail(string recipientName, string recipientEmail, string sessionName, DateTime sessionDate)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Annie Team", ""));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));

            var body = $"Dear {recipientName},\n\n" +
                $"You have successfully booked a session: {sessionName} on {sessionDate.ToString("MMMM dd, yyyy")}.\n\n" +
                "Thank you for choosing our service!\n\n" +
                "Best regards,\n" +
                "Annie Team";

            return true;
        }
    }
}
