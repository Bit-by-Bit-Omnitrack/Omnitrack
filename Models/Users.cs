using Microsoft.AspNetCore.Identity;

namespace UserRoles.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true; // Default to active
    }
}
