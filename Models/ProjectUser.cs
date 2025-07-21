using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class ProjectUser
    {
        public int ProjectUserId { get; set; }

        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }

        // The role of the user within this specific project
        // This is distinct from their global application roles.
        [Required]
        [StringLength(50)]
        public string ProjectRole { get; set; } // e.g., "Developer", "Project Manager", "QA"
    }
}