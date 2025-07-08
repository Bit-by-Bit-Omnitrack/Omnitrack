using System.ComponentModel.DataAnnotations;

namespace UserRoles.ViewModels
{
    public class RegisterViewModel
    {
        // This will store the user's ID if needed later (not required during registration)
        public int Id { get; set; }

        // Capture full name input (I'm keeping it as one field called "Name" for now)
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        // This will store the user's email address and enforce valid email formatting
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        // Password field — must be at least 8 characters but not more than 40
        // Also compares this with the ConfirmPassword field automatically
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Passwords do not match.")]
        public string Password { get; set; }

        // This field confirms the password entered above
        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        // Dropdown value from the form where the user selects their role
        // Admins can use this to assign specific roles during registration
        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; }

        // Whether or not the user should be active right after registration (can be auto set to true for now)
        public bool IsActive { get; set; } = true;  // default to true so new users can log in unless approval is required
    }
}
