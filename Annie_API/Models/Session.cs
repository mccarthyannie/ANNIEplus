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
<<<<<<< HEAD
<<<<<<< HEAD
    public required string Instructor { get; set; }
=======
    public required string Instructors { get; set; } = "Default Instructor";
>>>>>>> 7b48f7899f1128450c976cea42a1ca607c41cf5a
=======
    public required string Instructors { get; set; } = "Default Instructor";
>>>>>>> 0bd05a2afad5fa38bc7245178b9b62e3abeaa24d
    public string? Location { get; set; }
}