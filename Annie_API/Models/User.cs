using System;
using Microsoft.AspNetCore.Identity;

namespace Annie_API.Models;

public class User
{
	public required string Id { get; set; }
	public string Name { get; set; } = "DefaultUser";
	public string Email { get; set; } = string.Empty;
	public required string Password { get; set; }
	public UserRole Role { get; set; } = UserRole.User;
}
