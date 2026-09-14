-- Check if Admin role exists
DECLARE @AdminRoleId NVARCHAR(450)
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'

-- Create Admin role if it doesn't exist
IF @AdminRoleId IS NULL
BEGIN
    SET @AdminRoleId = 'ADMIN-ROLE-' + CONVERT(NVARCHAR(36), NEWID())
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@AdminRoleId, 'Admin', 'ADMIN', NEWID())
    PRINT 'Admin role created with ID: ' + @AdminRoleId
END
ELSE
BEGIN
    PRINT 'Admin role already exists with ID: ' + @AdminRoleId
END

-- Get the first user ID
DECLARE @UserId NVARCHAR(450)
SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'abdalrahmanalkharsa123@gmail.com'

-- Assign Admin role to the first user
IF @UserId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @AdminRoleId)
        PRINT 'Admin role assigned to: abdalrahmanalkharsa123@gmail.com'
    END
    ELSE
    BEGIN
        PRINT 'User already has Admin role'
    END
END
ELSE
BEGIN
    PRINT 'User not found'
END

-- Verify the change
SELECT 
    u.Email,
    r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'abdalrahmanalkharsa123@gmail.com';
