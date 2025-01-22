using System.ComponentModel.DataAnnotations;

namespace BeatIt.Models.DTOs
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(256, MinimumLength = 4)]
        public string Name { get; set; } = null!;
        [Required]
        [StringLength(256, MinimumLength = 8)]
        public string Password { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}