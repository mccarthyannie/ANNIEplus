using System.ComponentModel.DataAnnotations;

namespace Annie_API.DTOs
{
    public class BookingRequest
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = null!;

        [Required]
        public long SessionId { get; set; }
    }
}
