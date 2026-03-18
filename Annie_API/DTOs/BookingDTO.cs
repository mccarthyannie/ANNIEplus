using System;

public class BookingDTO
{
	public required long Id { get; set; }
	public long UserId { get; set; }
    public string UserName { get; set; } = "Default User";
	public long SessionId { get; set; }
	public string SessionName { get; set; } = "Default Session";
	public DateTime BookingDate { get; set; } = DateTime.UtcNow;
}
