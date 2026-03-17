using System;

namespace Annie_API.Models;
/*
 * Creates model class for the sessions that will be created or modified by the admins and viewed by the users
 */

public class Session
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public required DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; } = DateTime.MinValue;
    public required int Capacity { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Available;
    public required List<string> Instructors { get; set; }
    public string? Location { get; set; }
}