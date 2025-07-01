SELECT * FROM [UserRoles510].[dbo].[AspNetUsers]
WHERE 
Fullname IS NULL OR Fullname = ' '
OR PhoneNumber IS NULL OR PhoneNumber = ''