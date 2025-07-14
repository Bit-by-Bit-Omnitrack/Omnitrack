namespace UserRoles.Models
{
    public class EmailLog
    {
        public int Id { get; set; }

        public string Recipient { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsSuccess { get; set; }

        public string Status { get; set; } = string.Empty; 

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
