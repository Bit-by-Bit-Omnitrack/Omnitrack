using System.ComponentModel.DataAnnotations;

namespace Omnitrack.Models
{
    public class RegistorViewModel
    {
        public class RegisterViewModel
        {
            [Required]
            [EmailAddress]
            public required string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public required string Password { get; set; }

            [Required]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            [DataType(DataType.Password)]
            public required string ConfirmPassword { get; set; }
        }
    }
}
