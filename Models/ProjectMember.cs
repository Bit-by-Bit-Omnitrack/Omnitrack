using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class ProjectMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        // Navigation to the Project entity
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }

        [Required]
        public string UserId { get; set; }

        // Navigation to the User entity
        [ForeignKey(nameof(UserId))]
        public Users User { get; set; }

        // Project-specific role — distinct from global app roles
        [Required]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        public string ProjectRole { get; set; } // e.g., Developer, Manager, QA

       
        public DateTime JoinedDate { get; set; }
    }
}