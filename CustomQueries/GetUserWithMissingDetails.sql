SELECT TOP (100) 
[Id],
[FullName],
[Username],
[NormalizedUsername],
[Email],
[NormalizedEmail],
[EmailConfirmed],
[PasswordHash],
[SecurityStamp],
[ConcurrencyStamp],
[PhoneNumber],
[PhoneNumberConfirmed],
[TwoFactorEnabled],
[LockoutEnd],
[LockoutEnabled],
[AccessFailedCount],
[IsActive],
FROM [UserRoles510].[dbo].[AspNetUsers]
WHERE 
Fullname IS NULL OR Fullname = ' '
OR PhoneNumber IS NULL OR PhoneNumber = ''