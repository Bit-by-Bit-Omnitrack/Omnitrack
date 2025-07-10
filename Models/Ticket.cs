using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; } = 1;
        public string TaskID { get; set; }
        public string CreatedByID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public Users AssignedToUser { get; set; }
        [ForeignKey("CreatedByID")]
        public Users CreatedByUser { get; set; }
    }

    }
