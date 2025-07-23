using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class Chats
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public string Sender { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; }

        // 🏷 Chat status tracking: "New", "Seen", "Replied"
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "New";

        // 📎 File attachment path
        public string? AttachmentPath { get; set; }

        // 🔐 Role tag (used for filtering or tracking user type)
        public string? RoleTag { get; set; }

        // 👤 Computed property: sender initials
        [NotMapped]
        public string Initials =>
            !string.IsNullOrWhiteSpace(Sender) && Sender.Length >= 2
                ? Sender.Substring(0, 2).ToUpper()
                : "??";

        // 📐 Computed property: message alignment for UI
        [NotMapped]
        public string Alignment =>
            Sender == "Anonymous" ? "left" : "right"; // Customize logic if needed
    }
}