using System.ComponentModel.DataAnnotations;

namespace UserRoles.Models
{
    public class ChecklistItem
    {
        public int Id { get; set; }
        public int ItemID { get; set; }

        // Foreign Key
        public int ChecklistID { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IsCompleted { get; set; }

        // Navigation Property

       public Checklist Checklist { get; set; }

        public Checklists Checklists { get; set; }
    }
}
