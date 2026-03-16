namespace Annie_API.DTOs;

public class UserDTO
{
    public required long Id { get; set; }
    public string Name { get; set; } = "DefaultUser";
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
}
