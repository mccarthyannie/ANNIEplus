using Annie_API.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Annie_API.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public long Id { get; set; }
        [ForeignKey(nameof(User))]
        [Required]
        public string UserId { get; set; } = null!;
        public User? User { get; set; }
        [ForeignKey(nameof(Session))]
        [Required]
        public long SessionId { get; set; }
        public Session? Session { get; set; }
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    }
}
