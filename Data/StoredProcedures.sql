CREATE PROCEDURE [dbo].[sp_CreateUser]
    @Email NVARCHAR(256),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20),
    @PasswordHash NVARCHAR(MAX),
    @IsActive BIT,
    @CreatedDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert into AspNetUsers
        INSERT INTO [dbo].[AspNetUsers]
        (
            [Email],
            [FirstName],
            [LastName],
            [PhoneNumber],
            [PasswordHash],
            [IsActive],
            [CreatedDate],
            [UserName],
            [EmailConfirmed],
            [PhoneNumberConfirmed],
            [TwoFactorEnabled],
            [LockoutEnabled],
            [AccessFailedCount]
        )
        VALUES
        (
            @Email,
            @FirstName,
            @LastName,
            @PhoneNumber,
            @PasswordHash,
            @IsActive,
            @CreatedDate,
            @Email,
            1, -- EmailConfirmed
            0, -- PhoneNumberConfirmed
            0, -- TwoFactorEnabled
            1, -- LockoutEnabled
            0  -- AccessFailedCount
        );
        
        -- Get the newly created user's ID
        DECLARE @UserId NVARCHAR(450) = SCOPE_IDENTITY();
        
        -- Assign default role (User)
        INSERT INTO [dbo].[AspNetUserRoles] (UserId, RoleId)
        SELECT @UserId, Id
        FROM [dbo].[AspNetRoles]
        WHERE [Name] = 'User';
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 1 AS Success, 'User created successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Return error
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END

CREATE PROCEDURE [dbo].[sp_CreateUserWithEmail]
    @Email NVARCHAR(256),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20),
    @PasswordHash NVARCHAR(MAX),
    @IsActive BIT,
    @CreatedDate DATETIME2,
    @TemporaryPassword NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert into AspNetUsers
        INSERT INTO [dbo].[AspNetUsers]
        (
            [Email],
            [FirstName],
            [LastName],
            [PhoneNumber],
            [PasswordHash],
            [IsActive],
            [CreatedDate],
            [UserName],
            [EmailConfirmed],
            [PhoneNumberConfirmed],
            [TwoFactorEnabled],
            [LockoutEnabled],
            [AccessFailedCount]
        )
        VALUES
        (
            @Email,
            @FirstName,
            @LastName,
            @PhoneNumber,
            @PasswordHash,
            @IsActive,
            @CreatedDate,
            @Email,
            1, -- EmailConfirmed
            0, -- PhoneNumberConfirmed
            0, -- TwoFactorEnabled
            1, -- LockoutEnabled
            0  -- AccessFailedCount
        );
        
        -- Get the newly created user's ID
        DECLARE @UserId NVARCHAR(450) = SCOPE_IDENTITY();
        
        -- Assign default role (User)
        INSERT INTO [dbo].[AspNetUserRoles] (UserId, RoleId)
        SELECT @UserId, Id
        FROM [dbo].[AspNetRoles]
        WHERE [Name] = 'User';
        
        -- Insert into EmailLog for tracking
        INSERT INTO [dbo].[EmailLogs]
        (
            [UserId],
            [EmailType],
            [Subject],
            [Body],
            [SentDate],
            [Status]
        )
        VALUES
        (
            @UserId,
            'Welcome',
            'Welcome to Product Workflow System',
            'Dear ' + @FirstName + ' ' + @LastName + ',<br/><br/>' +
            'Your account has been created successfully. Please use the following credentials to login:<br/>' +
            'Email: ' + @Email + '<br/>' +
            'Temporary Password: ' + @TemporaryPassword + '<br/><br/>' +
            'Please change your password after your first login.<br/><br/>' +
            'Best regards,<br/>Product Workflow Team',
            GETDATE(),
            'Pending'
        );
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 1 AS Success, 'User created successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Return error
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END 