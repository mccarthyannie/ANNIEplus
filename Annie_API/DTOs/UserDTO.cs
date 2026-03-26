namespace Annie_API.DTOs;

public class UserDTO
{
    public required string Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
}
