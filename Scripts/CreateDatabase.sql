-- Script para crear la base de datos y asignar permisos al usuario Windows
-- Ejecutar contra la instancia (localdb)\MSSQLLocalDB

-- 1) Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TheConsortiumDB')
BEGIN
	PRINT 'Creating database TheConsortiumDB...';
	CREATE DATABASE [TheConsortiumDB];
END
ELSE
BEGIN
	PRINT 'Database TheConsortiumDB already exists.';
END
GO

-- 2) Crear login de Windows si no existe
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'YSMAEL\\ysmae')
BEGIN
	PRINT 'Creating server login for YSMAEL\\ysmae...';
	CREATE LOGIN [YSMAEL\\ysmae] FROM WINDOWS;
END
ELSE
BEGIN
	PRINT 'Server login YSMAEL\\ysmae already exists.';
END
GO

-- 3) Crear el usuario en la base de datos y asignarle rol db_owner
USE [TheConsortiumDB];
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'YSMAEL\\ysmae')
BEGIN
	PRINT 'Creating database user for YSMAEL\\ysmae...';
	CREATE USER [YSMAEL\\ysmae] FOR LOGIN [YSMAEL\\ysmae];
END
ELSE
BEGIN
	PRINT 'Database user YSMAEL\\ysmae already exists.';
END
GO

PRINT 'Granting db_owner role to YSMAEL\\ysmae...';
EXEC sp_addrolemember N'db_owner', N'YSMAEL\\ysmae';
GO

PRINT 'Script finished.';
