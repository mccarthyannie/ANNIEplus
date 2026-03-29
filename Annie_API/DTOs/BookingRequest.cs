using System.ComponentModel.DataAnnotations;

namespace Annie_API.DTOs
{
    public class BookingRequest
    {
        [Required] 
        public long SessionId { get; set; }
    }
}
