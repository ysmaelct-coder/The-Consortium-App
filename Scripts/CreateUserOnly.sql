-- Script para crear solo el usuario de base de datos en TheConsortiumDB
-- No crea el login de servidor. Ejecutar contra la instancia (localdb)\MSSQLLocalDB

IF DB_ID(N'TheConsortiumDB') IS NULL
BEGIN
	PRINT 'La base de datos TheConsortiumDB no existe. Ejecuta primero CreateDatabase.sql o crea la base.';
	RETURN;
END
GO

USE [TheConsortiumDB];
GO

-- Comprobar si el login de Windows existe en el servidor
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'YSMAEL\\ysmae')
BEGIN
	PRINT 'El login de servidor YSMAEL\\ysmae no existe. Crea el login con CREATE LOGIN [YSMAEL\\ysmae] FROM WINDOWS;';
END
ELSE
BEGIN
	-- Crear el usuario en la base de datos si no existe
	IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'YSMAEL\\ysmae')
	BEGIN
		CREATE USER [YSMAEL\\ysmae] FOR LOGIN [YSMAEL\\ysmae];
		PRINT 'Usuario de base de datos YSMAEL\\ysmae creado.';
	END
	ELSE
	BEGIN
		PRINT 'Usuario de base de datos YSMAEL\\ysmae ya existe.';
	END

	-- Asignar rol db_owner (ajusta según privilegios necesarios)
	IF NOT EXISTS (SELECT * FROM sys.database_role_members drm
				   JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
				   JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
				   WHERE r.name = N'db_owner' AND m.name = N'YSMAEL\\ysmae')
	BEGIN
		EXEC sp_addrolemember N'db_owner', N'YSMAEL\\ysmae';
		PRINT 'Rol db_owner asignado a YSMAEL\\ysmae.';
	END
	ELSE
	BEGIN
		PRINT 'El usuario ya es miembro de db_owner.';
	END
END
GO

PRINT 'Script finalizado.';

