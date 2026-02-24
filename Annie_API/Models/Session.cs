using System;

namespace Annie_API.Models;
/*
 * Creates model class for the sessions that will be created or modified by the admins and viewed by the users
 */

public class Session
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; } = DateTime.MinValue;
    public int Capacity { get; set; }
    public int Enrolled { get; set; }
    public SessionStatus Status { get; set; }
    public List<string> Instructors { get; set; }
    public string Location { get; set; }
}