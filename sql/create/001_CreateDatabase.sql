SET QUOTED_IDENTIFIER ON;

--
-- Create database if it doesn't already exist
--
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'#{app-DatabaseName}#')
BEGIN
	
	PRINT 'Creating Database #{app-DatabaseName}#';

	CREATE DATABASE [#{app-DatabaseName}#]
	(
		SERVICE_OBJECTIVE = ELASTIC_POOL ( name = "#{product-SqlServerElasticPool}#" ) 
	);
END;
