namespace Annie_API.DTOs;

public class UserDTO
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
}
