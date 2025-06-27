namespace UserRoles.Models
{
    public class User : UserTask
    {
        public int Id { get; set; }

        public string EmployeeNumber {  get; set; }
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Fullname => $"{FirstName} {MiddleName} {LastName}";

        public int PhoneNumber { get; set; }
        public string EmailAddress { get; set; }

        public string Country { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string Address { get; set; }

        public string Role { get; set; }
    }
}
