using Microsoft.AspNetCore.Identity;

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
        public UserApprovalStatus ApprovalStatus { get; set; } = UserApprovalStatus.Pending;
        public string? RejectionReason { get; set; }
    }
}
