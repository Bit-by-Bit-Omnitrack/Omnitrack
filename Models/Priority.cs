namespace UserRoles.Models
{
    public class Priority
    {
        public int Id { get; set; }
        public string Level { get; set; } // "High", "Medium", "Low"

        // Navigation
        public ICollection<TaskItem> TaskItems { get; set; }

    }
}
