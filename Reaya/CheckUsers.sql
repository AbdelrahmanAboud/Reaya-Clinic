-- Get all users and their roles
SELECT 
    u.Id,
    u.Email,
    u.FullName,
    u.PhoneNumber,
    r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.Email;
