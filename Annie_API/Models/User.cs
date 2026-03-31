using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Annie_API.Models;

public class User : IdentityUser
{
	[Required(ErrorMessage ="Name is required!")]
	public string Name { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.User;
}
