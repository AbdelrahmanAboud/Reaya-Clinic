-- Delete all patients and admin users, keep only doctors

-- Step 1: Get role IDs
DECLARE @AdminRoleId NVARCHAR(450)
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'

DECLARE @PatientRoleId NVARCHAR(450)
SELECT @PatientRoleId = Id FROM AspNetRoles WHERE Name = 'Patient'

DECLARE @DoctorRoleId NVARCHAR(450)
SELECT @DoctorRoleId = Id FROM AspNetRoles WHERE Name = 'Doctor'

-- Step 2: Delete role assignments for patients and admins
DELETE FROM AspNetUserRoles WHERE RoleId = @PatientRoleId
DELETE FROM AspNetUserRoles WHERE RoleId = @AdminRoleId

-- Step 3: Get user IDs to delete (users who are not doctors)
DECLARE @UserIdsToDelete TABLE (Id NVARCHAR(450))

INSERT INTO @UserIdsToDelete (Id)
SELECT u.Id 
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN Doctors d ON u.Id = d.UserId
WHERE (ur.RoleId IS NULL OR ur.RoleId = @PatientRoleId)
AND d.UserId IS NULL

-- Step 4: Delete related patients
DELETE FROM Patients WHERE UserId IN (SELECT Id FROM @UserIdsToDelete)

-- Step 5: Delete users
DELETE FROM AspNetUsers WHERE Id IN (SELECT Id FROM @UserIdsToDelete)

-- Step 6: Delete roles
DELETE FROM AspNetRoles WHERE Name = 'Patient'
DELETE FROM AspNetRoles WHERE Name = 'Admin'

-- Step 7: Verify remaining users
SELECT 
    u.Email,
    u.FullName,
    r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.Email;

PRINT 'Cleanup completed. Only doctors remain.'
