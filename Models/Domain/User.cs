using System;

namespace UserRoles.Models.Domain
{
    // This class represents the structure of a custom domain user in my app
    public class User
    {
        public Guid Id { get; set; } // Primary key for each user

        public string FirstName { get; set; } // Stores the user's first name

        public string LastName { get; set; } // Stores the user's last name

        public string EmailAddress { get; set; } // This will hold their email

        public string PhoneNumber { get; set; } // User's phone number if needed

        public string Role { get; set; } // This is for the role I assign manually

        public bool IsActive { get; set; } = false; // Added this to control login access after admin approval

        public DateTime CreatedOn { get; set; } // To track when the record was created

        public DateTime? ModifiedOn { get; set; } // Nullable - only updated on edit
    }
}
