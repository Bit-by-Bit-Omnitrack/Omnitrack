using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserRoles.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property for members assigned to project
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        // Navigation property for taks assigned to project 
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();

        // Navigation property for tickets associated with the project
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}