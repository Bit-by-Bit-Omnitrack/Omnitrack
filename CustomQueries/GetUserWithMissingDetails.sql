SELECT 
    Id,
    EmployeeNumber,
    FirstName,
    MiddleName,
    LastName,
    PhoneNumber,
    EmailAddress,
    Country,
    DateOfBirth,
    Address,
    Role
FROM Users
WHERE 
    EmployeeNumber IS NULL OR EmployeeNumber = ''
    OR FirstName IS NULL OR FirstName = ''
    OR LastName IS NULL OR LastName = ''
    OR EmailAddress IS NULL OR EmailAddress = ''
    OR PhoneNumber IS NULL
    OR Country IS NULL OR Country = ''
    OR Address IS NULL OR Address = ''
    OR Role IS NULL OR Role = '';