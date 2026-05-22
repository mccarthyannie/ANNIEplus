using System.ComponentModel.DataAnnotations;

namespace Annie_API.DTOs
{
    public class CheckInDTO
    {
        [Required]
        [EmailAddress]
        public string userEmail { get; set; } = null!;

        [Required]
        public long sessionId { get; set; }
    }
}
