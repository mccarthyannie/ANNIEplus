using System;
using System.ComponentModel.DataAnnotations;

public class BookingDTO
{
	[Required]
	public required long Id { get; set; }
	[Required]
	public string Email { get; set; } = null!;
    [Required]
    public long SessionId { get; set; } 
    public string SessionName { get; set; } = "Default Session";
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
}
