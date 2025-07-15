namespace UserRoles.Models
{
    public class Priority
    {
        public int Id { get; set; }
        public string Level { get; set; } // "High", "Medium", "Low"

        public string Color { get; set; } // Hex color code for the priority level
        // Navigation
        public ICollection<TaskItem> TaskItems { get; set; }

    }
}
