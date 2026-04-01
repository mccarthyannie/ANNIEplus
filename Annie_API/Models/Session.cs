using System;
using System.ComponentModel.DataAnnotations;

namespace Annie_API.Models;
/*
 * Creates model class for the sessions that will be created or modified by the admins and viewed by the users
 */

public class Session
{

    [Required]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public int Capacity { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Available;

    [Required]
    public required string Instructor { get; set; } = string.Empty;

    public string? Location { get; set; }

    public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();


}