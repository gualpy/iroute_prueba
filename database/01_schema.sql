IF DB_ID('iroute') IS NULL CREATE DATABASE iroute;
GO
USE iroute;
GO

-- Tabla principal (misma estructura que el CSV)
IF OBJECT_ID('dbo.commerce') IS NULL
CREATE TABLE dbo.commerce (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    pc_numdoc       VARCHAR(30)  NULL,
    pc_tipodoc      VARCHAR(10)  NULL,
    pc_nomcomred    VARCHAR(100) NULL,
    pc_razonsocial  VARCHAR(150) NULL,
    pc_direccion    VARCHAR(200) NULL,
    pc_telefono     VARCHAR(20)  NULL,
    pc_email        VARCHAR(100) NULL,
    pc_processdate  DATE         NOT NULL
);
CREATE INDEX IX_commerce_processdate ON dbo.commerce(pc_processdate);
GO

-- Cuarentena: misma estructura + motivo
IF OBJECT_ID('dbo.commerce_quarantine') IS NULL
CREATE TABLE dbo.commerce_quarantine (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    pc_numdoc       VARCHAR(30)  NULL,
    pc_tipodoc      VARCHAR(10)  NULL,
    pc_nomcomred    VARCHAR(100) NULL,
    pc_razonsocial  VARCHAR(150) NULL,
    pc_direccion    VARCHAR(200) NULL,
    pc_telefono     VARCHAR(20)  NULL,
    pc_email        VARCHAR(100) NULL,
    pc_processdate  DATE         NOT NULL
);
GO
IF COL_LENGTH('dbo.commerce_quarantine','motivo') IS NULL
    ALTER TABLE dbo.commerce_quarantine ADD motivo VARCHAR(500) NOT NULL DEFAULT '';
GO

-- Tipo tabla para carga masiva (una sola invocación del SP por archivo)
IF TYPE_ID('dbo.commerce_type') IS NULL
CREATE TYPE dbo.commerce_type AS TABLE (
    pc_numdoc       VARCHAR(30)  NULL,
    pc_tipodoc      VARCHAR(10)  NULL,
    pc_nomcomred    VARCHAR(100) NULL,
    pc_razonsocial  VARCHAR(150) NULL,
    pc_direccion    VARCHAR(200) NULL,
    pc_telefono     VARCHAR(20)  NULL,
    pc_email        VARCHAR(100) NULL,
    pc_processdate  DATE         NOT NULL
);
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @rows dbo.commerce_type READONLY
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.commerce (pc_numdoc, pc_tipodoc, pc_nomcomred, pc_razonsocial,
                              pc_direccion, pc_telefono, pc_email, pc_processdate)
    SELECT pc_numdoc, pc_tipodoc, pc_nomcomred, pc_razonsocial,
           pc_direccion, pc_telefono, pc_email, pc_processdate
    FROM @rows;
    SELECT @@ROWCOUNT AS inserted;
END
GO

-- Valida registros de una fecha, mueve inválidos a cuarentena y retorna la cantidad
CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Registros inválidos con su motivo
    SELECT c.id,
           RTRIM(CONCAT(
             IIF(c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = '',
                 'El nombre del comercio (nomcomred) se encuentra vacio. ', ''),
             IIF(c.pc_numdoc IS NULL OR LTRIM(RTRIM(c.pc_numdoc)) = '',
                 'El número (numdoc) se encuentra vacio. ', ''),
             IIF(c.pc_numdoc LIKE '%[A-Za-z]%',
                 'El número (numdoc) contiene letras. ', ''),
             IIF(c.pc_numdoc LIKE '%[^A-Za-z0-9]%',
                 'El número (numdoc) contiene caracteres especiales. ', '')
           )) AS motivo
    INTO #invalid
    FROM dbo.commerce c
    WHERE c.pc_processdate = @processdate
      AND (   c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = ''
           OR c.pc_numdoc IS NULL OR LTRIM(RTRIM(c.pc_numdoc)) = ''
           OR c.pc_numdoc LIKE '%[^0-9]%');

    BEGIN TRAN;

    INSERT INTO dbo.commerce_quarantine
        (pc_numdoc, pc_tipodoc, pc_nomcomred, pc_razonsocial, pc_direccion,
         pc_telefono, pc_email, pc_processdate, motivo)
    SELECT c.pc_numdoc, c.pc_tipodoc, c.pc_nomcomred, c.pc_razonsocial, c.pc_direccion,
           c.pc_telefono, c.pc_email, c.pc_processdate, i.motivo
    FROM dbo.commerce c
    JOIN #invalid i ON i.id = c.id;

    DECLARE @count INT = @@ROWCOUNT;

    DELETE c FROM dbo.commerce c JOIN #invalid i ON i.id = c.id;

    COMMIT;

    SELECT @count AS quarantined;
END
GO
