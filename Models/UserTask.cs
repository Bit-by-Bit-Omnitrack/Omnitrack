namespace UserRoles.Models
{
    public class UserTask
    {
        public string? CreatedByID { get; set; }

        public DateTime CreatedOn{ get; set; }

        public string? ModifiedByID { get; set; }
        public DateTime ModifiedOn { get; set; }

    }
}
