
using System.ComponentModel.DataAnnotations;

namespace DTO.Users
{
    public class SetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
