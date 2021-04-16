IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_ConfirmAllocations') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_ConfirmAllocations
GO

CREATE PROCEDURE [dbo].[p_ConfirmAllocations]
    @RightsIssueKey UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

        -- Use table names by using the share purchase plan key (GUID) to make them unique.
        DECLARE @v_TableAllocationStaging VARCHAR(255) = 'Allocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')
        DECLARE @v_TableCustodianAllocationStaging VARCHAR(255) = 'CustodianAllocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')


        EXEC('IF NOT EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = ''' + @v_TableAllocationStaging + '''
            ) 
            THROW 51000, ''Allocations do not exist. Please calculate the allocations before confirming.'', 1; ')
        
        EXEC('IF NOT EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = ''' + @v_TableCustodianAllocationStaging + '''
            ) 
            THROW 51000, ''Custodian Allocations do not exist. Please calculate the allocations before confirming.'', 1; ')

		BEGIN TRANSACTION

        -- insert records into  allocations table from temporary staging tables
        EXEC('INSERT INTO dbo.Allocation
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
            FROM dbo.' + @v_TableAllocationStaging)

		-- insert records into Custodian allocations table from temporary staging tables
		EXEC('INSERT INTO dbo.CustodianAllocation 
            (
                CustodianAcceptanceId,
                a.AllocationId,
                EntitledShares,
                AdditionalShares,
                AllocatedAmount,
                RefundAmount,
                ResidualAmount,
                LastUpdatedAtUtc
            )
            SELECT
                ca.CustodianAcceptanceId,
                a2.AllocationId,
                ca.EntitledShares,
                ca.AdditionalShares,
                ca.AllocatedAmount,
                ca.RefundAmount,
                ca.ResidualAmount,
                getutcdate()
            FROM dbo.' + @v_TableCustodianAllocationStaging + ' as ca
            INNER JOIN dbo.' + @v_TableAllocationStaging + ' as a
                ON ca.AllocationId = a.AllocationId
            INNER JOIN dbo.Allocation as a2 
                ON a.AllocationKey = a2.AllocationKey ')

        -- Drop existing tables as we have finished copying the data
        EXEC('DROP TABLE dbo.' + @v_TableAllocationStaging)
        EXEC('DROP TABLE dbo.' + @v_TableCustodianAllocationStaging)

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

GRANT EXECUTE ON dbo.p_ConfirmAllocations TO  [ServiceUsers];
GO