using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserRoles.Models
{
    public class Project
    {
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now; // Default to current date

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; } // Nullable if end date can be undecided

        public bool IsActive { get; set; } = true;

        // Navigation property for users in this project
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    }
}