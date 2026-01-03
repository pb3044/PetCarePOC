-- Fix User Roles Script
-- This script creates roles and assigns them to existing users
-- Run this script on your database to fix access issues

USE [PetCarePlatform]
GO

-- Create Roles if they don't exist
SET IDENTITY_INSERT [dbo].[AspNetRoles] ON
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin')
BEGIN
    INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [Description], [CreatedAt]) 
    VALUES (1, N'Admin', N'ADMIN', NEWID(), N'Administrator role', CAST(N'2025-06-22T13:00:00.0000000' AS DateTime2))
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'PetOwner')
BEGIN
    INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [Description], [CreatedAt]) 
    VALUES (2, N'PetOwner', N'PETOWNER', NEWID(), N'Pet Owner role', CAST(N'2025-06-22T13:00:00.0000000' AS DateTime2))
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'ServiceProvider')
BEGIN
    INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [Description], [CreatedAt]) 
    VALUES (3, N'ServiceProvider', N'SERVICEPROVIDER', NEWID(), N'Service Provider role', CAST(N'2025-06-22T13:00:00.0000000' AS DateTime2))
END
GO
SET IDENTITY_INSERT [dbo].[AspNetRoles] OFF
GO

-- Assign Roles to Users based on UserType
-- Service Providers (UserType = 1): Users 1-5
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 1 AND r.[Name] = 'ServiceProvider')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 1, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'ServiceProvider' -- Jane Doe
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 2 AND r.[Name] = 'ServiceProvider')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 2, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'ServiceProvider' -- John Smith
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 3 AND r.[Name] = 'ServiceProvider')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 3, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'ServiceProvider' -- Emily Nguyen
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 4 AND r.[Name] = 'ServiceProvider')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 4, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'ServiceProvider' -- Carlos Lopez
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 5 AND r.[Name] = 'ServiceProvider')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 5, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'ServiceProvider' -- Aisha Khan
END
GO

-- Pet Owners (UserType = 0): Users 6-10
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 6 AND r.[Name] = 'PetOwner')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 6, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'PetOwner' -- Samantha Lee
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 7 AND r.[Name] = 'PetOwner')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 7, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'PetOwner' -- Michael Brown
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 8 AND r.[Name] = 'PetOwner')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 8, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'PetOwner' -- Priya Sharma
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 9 AND r.[Name] = 'PetOwner')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 9, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'PetOwner' -- Daniel Nguyen
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] ur 
               INNER JOIN [dbo].[AspNetRoles] r ON ur.[RoleId] = r.[Id] 
               WHERE ur.[UserId] = 10 AND r.[Name] = 'PetOwner')
BEGIN
    INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) 
    SELECT 10, [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'PetOwner' -- Fatima Ali
END
GO

PRINT 'Roles created and assigned successfully!'
GO

