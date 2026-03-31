using System.ComponentModel.DataAnnotations;

namespace Annie_API.DTOs
{
    public class EmailDTO
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = null!; 
    }
}
