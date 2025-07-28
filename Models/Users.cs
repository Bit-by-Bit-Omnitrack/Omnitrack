using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

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

        public bool IsActive { get; set; } = false;

        public bool IsApproved { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? ModifiedOn { get; set; }

        public UserApprovalStatus ApprovalStatus { get; set; } = UserApprovalStatus.Pending;

        public string? RejectionReason { get; set; }

        // Navigation property for projects this user is assigned to
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    }
}