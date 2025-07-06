namespace UserRoles.Models
{
    public class Checklists
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ItemID { get; set; }

       
        public string Description { get; set; }

        public bool IsCompleted { get; set; }

     
    }
}
