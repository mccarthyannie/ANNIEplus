using System.ComponentModel.DataAnnotations;

namespace Annie_API.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage ="You must enter a valid e-mail.")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage ="Password is not long enough.")]
        public string Password { get; set; } = null!;
    }
}
