using UserRoles.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserRoles.ViewModels
{
    public class ProjectViewModel
    {
        public Project Project { get; set; }
        public IEnumerable<ProjectUserViewModel> ProjectUsers { get; set; }
    }

    public class ProjectUserViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ProjectRole { get; set; }
        public int ProjectId { get; set; }
    }

    public class AssignUsersToProjectViewModel
    {
        public int ProjectId { get; set; }
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        public List<UserSelectionViewModel> AvailableUsers { get; set; }
        public List<string> SelectedUserIds { get; set; }
        public string RoleForSelectedUsers { get; set; } // For assigning a role to newly added users
    }

    public class UserSelectionViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
        public string CurrentProjectRole { get; set; } // To display if already assigned
    }

    public class EditProjectUserRoleViewModel
    {
        public int ProjectUserId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Project Role")]
        public string ProjectRole { get; set; } // e.g., "Developer", "Project Manager", "QA"
    }
}