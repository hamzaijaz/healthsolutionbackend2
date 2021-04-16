IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_CreateAllocations') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_CreateAllocations
GO

-- drop the existing table type definition 
if exists (
	select 1
	from sys.types t
	inner join sys.schemas s
		on s.schema_id = t.schema_id
	where 
		t.is_user_defined = 1
		and t.is_table_type = 1
		and s.name = 'dbo'
		and t.name = 't_Allocation')

	drop type dbo.t_Allocation

go

if exists (
	select 1
	from sys.types t
	inner join sys.schemas s
		on s.schema_id = t.schema_id
	where 
		t.is_user_defined = 1
		and t.is_table_type = 1
		and s.name = 'dbo'
		and t.name = 't_CustodianAllocation')

	drop type dbo.t_CustodianAllocation

go

CREATE TYPE [dbo].[t_Allocation] AS TABLE(
    [AllocationKey] UNIQUEIDENTIFIER NOT NULL,
    [OfferId] int NOT NULL,
    [EntitledShares] bigint NOT NULL,
    [AdditionalShares] bigint NOT NULL,
	[AllocatedAmount] decimal(14,2) NOT NULL,
    [RefundAmount] decimal(14,2) NOT NULL,
    [ResidualAmount] decimal(14,2) NOT NULL,
    [ScalebackRatio] decimal(9,8) NOT NULL,
    [MinimumGuaranteedShares] bigint NOT NULL,
    [SharesRounding] varchar(10) NOT NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.t_Allocation TO  [ServiceUsers];
GO

CREATE TYPE [dbo].[t_CustodianAllocation] AS TABLE(
    [AllocationKey] UNIQUEIDENTIFIER NOT NULL,
    [CustodianAcceptanceId] int NOT NULL,
    [EntitledShares] bigint NOT NULL,
    [AdditionalShares] bigint NOT NULL,
    [AllocatedAmount] decimal(14,2) NOT NULL,
    [RefundAmount] decimal(14,2) NOT NULL,
    [ResidualAmount] decimal(14,2) NOT NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.t_CustodianAllocation TO  [ServiceUsers];
GO

CREATE PROCEDURE [dbo].[p_CreateAllocations]
    @RightsIssueKey UNIQUEIDENTIFIER,
    @Allocations dbo.t_Allocation READONLY,
    @CustodianAllocations dbo.t_CustodianAllocation READONLY
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

        -- Generate table names by using the share purchase plan key (GUID) to make them unique.
        DECLARE @v_TableAllocationStaging VARCHAR(255) = 'Allocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')
        DECLARE @v_TableCustodianAllocationStaging VARCHAR(255) = 'CustodianAllocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')

		BEGIN TRANSACTION

        -- Drop existing tables and create new tables. We don't want any existing data.
        EXEC('IF EXISTS (
                SELECT 1 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = ''' + @v_TableAllocationStaging + '''
            ) 
            DROP TABLE dbo.' + @v_TableAllocationStaging)

        EXEC('SELECT TOP 0 * INTO dbo.' + @v_TableAllocationStaging + ' From Allocation')

        EXEC('IF EXISTS (
                SELECT 1 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = ''' + @v_TableCustodianAllocationStaging + '''
            ) 
            DROP TABLE dbo.' + @v_TableCustodianAllocationStaging)

        EXEC('SELECT TOP 0 * INTO dbo.' + @v_TableCustodianAllocationStaging + ' From CustodianAllocation')

        DECLARE @sql nvarchar(max);

        -- insert records into  allocations staging table
        SET @sql = 'INSERT INTO dbo.' + @v_TableAllocationStaging + ' 
            (
                AllocationKey,
                OfferId,
                EntitledShares,
                AdditionalShares,
				AllocatedAmount,
                RefundAmount,
                ResidualAmount,
                ScalebackRatio,
                MinimumGuaranteedShares,
                SharesRounding,
                LastUpdatedAtUtc 
            )
            SELECT
                AllocationKey,
                OfferId,
                EntitledShares,
                AdditionalShares,
				AllocatedAmount,
                RefundAmount,
                ResidualAmount,
                ScalebackRatio,
                MinimumGuaranteedShares,
                SharesRounding,
                getutcdate() 
            FROM @Allocations'

        EXEC sp_executesql @sql, N'@Allocations dbo.t_Allocation READONLY', @Allocations

		-- insert records into Custodian allocations staging table
		SET @sql = 'INSERT INTO dbo.' + @v_TableCustodianAllocationStaging + '
            (
                CustodianAcceptanceId,
                AllocationId,
                EntitledShares,
                AdditionalShares,
                AllocatedAmount,
                RefundAmount,
                ResidualAmount,
                LastUpdatedAtUtc
            )
            SELECT
                ca.CustodianAcceptanceId,
                a.AllocationId,
                ca.EntitledShares,
                ca.AdditionalShares,
                ca.AllocatedAmount,
                ca.RefundAmount,
                ca.ResidualAmount,
                getutcdate()
            FROM @CustodianAllocations as ca
            INNER JOIN dbo.' + @v_TableAllocationStaging + ' as a 
                ON ca.AllocationKey = a.AllocationKey '
		
        EXEC sp_executesql @sql, N'@CustodianAllocations dbo.t_CustodianAllocation READONLY', @CustodianAllocations

		COMMIT TRANSACTION
		
        RETURN (0)

	END TRY
	
	BEGIN CATCH
	
		if @@TRANCOUNT > 0
	        ROLLBACK TRANSACTION

		DECLARE	@errorMessage						NVARCHAR(800)   
				,@errorNumber						INT  
				,@errorSeverity						INT  
				,@errorState						INT  
				,@errorLine							INT  
				,@errorProcedure					NVARCHAR(255)


		SELECT	@errorNumber = error_number()
				,@errorSeverity = error_severity()
				,@errorState = error_state()
				,@errorLine = error_line()
				,@errorProcedure = isnull(error_procedure(), '-')   
  
		SELECT	@errorMessage = N'Error %d, Level %d, state %d, in procedure %s, at line %d.' +   
								'The underlying message was: '+ error_message();   
       
		RAISERROR
			(   
			@errorMessage,   
			@errorSeverity,   
			1,   
			@errorNumber,   
			@errorSeverity,   
			@errorState,   
			@errorProcedure,   
			@errorLine   
			);

	END CATCH
	
END
GO

GRANT ALTER ON SCHEMA::dbo TO [ServiceUsers];
GO

GRANT CREATE TABLE TO [ServiceUsers];
GO

GRANT EXECUTE ON dbo.p_CreateAllocations TO  [ServiceUsers];
GO