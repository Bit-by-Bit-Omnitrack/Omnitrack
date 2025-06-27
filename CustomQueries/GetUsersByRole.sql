SELECT 
    Id,
    EmployeeNumber,
    FirstName,
    MiddleName,
    LastName,
    CONCAT(FirstName, ' ', MiddleName, ' ', LastName) AS Fullname,
    PhoneNumber,
    EmailAddress,
    Country,
    DateOfBirth,
    Address,
    Role
FROM Users
WHERE Role = 'Admin';
