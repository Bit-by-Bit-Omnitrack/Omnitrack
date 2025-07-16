using UserRoles.Models;

namespace UserRoles.ViewModels
{
    public class UserWithRolesViewModel
    {
        public Users User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
