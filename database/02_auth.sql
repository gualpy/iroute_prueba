USE iroute;
GO

IF OBJECT_ID('dbo.app_user') IS NULL
CREATE TABLE dbo.app_user (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    username      VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(200) NOT NULL
);
GO

CREATE OR ALTER PROCEDURE dbo.sp_get_user_hash
    @username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT password_hash FROM dbo.app_user WHERE username = @username;
END
GO

-- Crea el usuario solo si no existe (el hash lo calcula la API)
CREATE OR ALTER PROCEDURE dbo.sp_ensure_user
    @username VARCHAR(50),
    @password_hash VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.app_user WHERE username = @username)
        INSERT INTO dbo.app_user (username, password_hash) VALUES (@username, @password_hash);
END
GO
