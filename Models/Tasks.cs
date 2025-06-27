namespace UserRoles.Models
{
    public class Tasks : UserTask
    {
        public int Id { get; set; }
        public string TaskName { get; set; }

        public string Description { get; set; }

        public DateTime Due_Date { get; set; }

       

    }
}
