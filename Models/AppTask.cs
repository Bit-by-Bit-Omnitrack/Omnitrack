namespace UserRoles.Models
{
    public class AppTask
    {
        public int Id { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public int CreatedByID { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
