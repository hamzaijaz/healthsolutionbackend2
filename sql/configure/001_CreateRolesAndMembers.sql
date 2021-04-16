IF DB_NAME() = N'#{app-DatabaseName}#'
BEGIN
	-- If the Deploy Account exists as a user on this database, make it a db_owner
	-- Else we assume the Deploy Account created the database and is the dbo
	-- IF EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{product-kv-SqlDeploymentUserName}#')	
	-- BEGIN
	-- 	PRINT 'Set DB Owner: #{product-kv-SqlDeploymentUserName}#';
	-- 	ALTER ROLE [db_owner] ADD MEMBER [#{product-kv-SqlDeploymentUserName}#];
	-- END
		
	-- Create new Roles
	IF NOT EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'ServiceUsers' AND type = 'R')
	BEGIN
		PRINT 'Create Role: ServiceUsers';
		CREATE ROLE [ServiceUsers] AUTHORIZATION dbo;

		GRANT SELECT TO [ServiceUsers];
		GRANT INSERT TO [ServiceUsers];
		GRANT UPDATE TO [ServiceUsers];
		GRANT DELETE TO [ServiceUsers];
	END

	IF NOT EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'SupportUsers' AND type = 'R')
	BEGIN
		PRINT 'Create Role: SupportUsers';
		CREATE ROLE [SupportUsers] AUTHORIZATION dbo;

		GRANT SELECT TO [SupportUsers];
	END

	IF NOT EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'TestServiceUsers' AND type = 'R')
	BEGIN
		PRINT 'Create Role: TestServiceUsers';
		CREATE ROLE [TestServiceUsers] AUTHORIZATION dbo;

		GRANT SELECT TO [TestServiceUsers];
		GRANT INSERT TO [TestServiceUsers];
		GRANT UPDATE TO [TestServiceUsers];
		GRANT DELETE TO [TestServiceUsers];
	END


	-- Create users if they don't exist
	IF '#{app-DbRoleServiceUsersMember}#' <> 'NotSet'
	AND NOT EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleServiceUsersMember}#')
	BEGIN
		PRINT 'Create User: #{app-DbRoleServiceUsersMember}#';
		CREATE USER [#{app-DbRoleServiceUsersMember}#] WITH PASSWORD='#{secret-DbRoleServiceUsersPassword}#'
		GRANT CONNECT TO [#{app-DbRoleServiceUsersMember}#];
	END

	IF '#{app-DbRoleSupportUsersMember}#' <> 'NotSet'
	AND NOT EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleSupportUsersMember}#')
	BEGIN
		PRINT 'Create User: #{app-DbRoleSupportUsersMember}#';
		GRANT CONNECT TO [#{app-DbRoleSupportUsersMember}#];
	END

	IF '#{app-DbRoleTestServiceUsersMember}#' <> 'NotSet'
	AND '#{product-SqlServerEnvironment}#' NOT IN ('PROD')
	AND NOT EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleTestServiceUsersMember}#')
	BEGIN
		PRINT 'Create User: #{app-DbRoleTestServiceUsersMember}#';
		GRANT CONNECT TO [#{app-DbRoleTestServiceUsersMember}#];
	END

	------------------------
	-- Assign users to roles
	------------------------

	-- Service Users
	IF EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'ServiceUsers' AND type = 'R')
	AND EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleServiceUsersMember}#')
	AND '#{app-DbRoleServiceUsersMember}#' <> 'NotSet'
	BEGIN
		PRINT 'Add as Service Users: #{app-DbRoleServiceUsersMember}#';
		ALTER ROLE ServiceUsers ADD MEMBER [#{app-DbRoleServiceUsersMember}#];
	END

	-- Support Users are added as Service Users when NOT PROD
	IF EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'ServiceUsers' AND type = 'R')
	AND EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleSupportUsersMember}#')
	AND '#{app-DbRoleSupportUsersMember}#' <> 'NotSet'
	AND '#{product-SqlServerEnvironment}#' NOT IN ('PROD')
	BEGIN
		-- Add the configured Members to the Role if they both exist
		PRINT 'Add as Service Users: #{app-DbRoleSupportUsersMember}#';
		ALTER ROLE ServiceUsers ADD MEMBER [#{app-DbRoleSupportUsersMember}#];
	END

	-- Support Users, only when set
	IF EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'SupportUsers' AND type = 'R')
	AND EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleSupportUsersMember}#')
	AND '#{app-DbRoleSupportUsersMember}#' <> 'NotSet'
	BEGIN
		-- Add the configured Members to the Role if they both exist
		PRINT 'Add as Support Users: #{app-DbRoleSupportUsersMember}#';
		ALTER ROLE SupportUsers ADD MEMBER [#{app-DbRoleSupportUsersMember}#];
	END

	-- Test Service Users, only when set and not in prod
	IF EXISTS (SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = 'TestServiceUsers' AND type = 'R')
	AND EXISTS(SELECT 1 FROM [#{app-DatabaseName}#].[sys].[database_principals] WHERE name = '#{app-DbRoleTestServiceUsersMember}#')
	AND '#{app-DbRoleTestServiceUsersMember}#' <> 'NotSet'
	AND '#{product-SqlServerEnvironment}#' NOT IN ('PROD')
	BEGIN
		-- Add the configured Members to the Role
		PRINT 'Add as Test Service Users: #{app-DbRoleTestServiceUsersMember}#';
		ALTER ROLE TestServiceUsers ADD MEMBER [#{app-DbRoleTestServiceUsersMember}#];
	END
END