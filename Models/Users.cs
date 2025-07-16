using Microsoft.AspNetCore.Identity;
using System;

namespace UserRoles.Models
{

    public enum UserApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; } = false; // Default to active

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? ModifiedOn { get; set; }
        public UserApprovalStatus ApprovalStatus { get; set; } = UserApprovalStatus.Pending;
        public string? RejectionReason { get; set; }
    }
}
