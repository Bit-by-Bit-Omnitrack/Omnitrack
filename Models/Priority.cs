namespace UserRoles.Models
{
    public class Priority
    {
        public int Id { get; set; }
        public string Name { get; set; } // "High", "Medium", "Low"
        public string Color { get; set; }

        // Navigation
        public ICollection<TaskItem> TaskItems { get; set; }

    }
}
