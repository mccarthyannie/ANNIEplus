using System.ComponentModel.DataAnnotations;

namespace Annie_API.DTOs
{
    public class PasswordResetDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public string Token { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;
    }
}
