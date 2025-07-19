    namespace UserRoles.Models
    {
        public class Chats
        {
                public int Id { get; set; }
                public int TicketId { get; set; }
                public string Sender { get; set; }
                public string Message { get; set; }
                public DateTime SentAt { get; set; }
        

        }
    }
