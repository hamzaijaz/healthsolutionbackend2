IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_CreateCustodianAcceptances') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_CreateCustodianAcceptances
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
		and t.name = 't_Acceptance')

	drop type dbo.t_Acceptance

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
		and t.name = 't_CustodianAcceptance')

	drop type dbo.t_CustodianAcceptance

go
 
CREATE TYPE [dbo].[t_CustodianAcceptance] AS TABLE(
	[CustodianId] [int] NOT NULL,
	[CustodianInvestorId] [varchar](20) NOT NULL,
	[FullName] [varchar](200) NOT NULL,
	[Address1] [varchar](200) NOT NULL,
	[Address2] [varchar](200) NOT NULL,
	[Address3] [varchar](200) NOT NULL,
	[Address4] [varchar](200) NOT NULL,
	[Address5] [varchar](200) NOT NULL,
	[Address6] [varchar](200) NOT NULL,
	[Postcode] [varchar](10) NOT NULL,
	[CountryCode] [varchar](3) NOT NULL,
	[EmailAddress] [varchar](100) NOT NULL,
	[MobileNumber] [varchar](20) NOT NULL,
	[TotalShares] [bigint] NOT NULL,
	[InvestmentAmount] [decimal](14,2) NOT NULL,
	[AdditionalShares] [bigint] NOT NULL,
	[EntitledShares] [bigint] NOT NULL,
	[OfferId] [int] NOT NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.t_CustodianAcceptance TO  [ServiceUsers];
GO

CREATE PROCEDURE [dbo].[p_CreateCustodianAcceptances]
    @RightsIssueId int,
	@CustodianId int,
    @CustodianAcceptances dbo.t_CustodianAcceptance READONLY
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		DECLARE @v_LastUpdatedAtUtc DateTime = getutcdate();

		CREATE TABLE #AcceptancesToDelete 
		(
			CustodianAcceptanceId int
		);
	
	-- Gather acceptances which need to be deleted
	-- note: keep reads on other tables outside transaction scope

		INSERT INTO #AcceptancesToDelete
		SELECT ca.CustodianAcceptanceId
		From dbo.CustodianAcceptance ca
		JOIN dbo.Offer o
			ON ca.OfferId = o.OfferId
		JOIN dbo.RightsIssue r
			ON o.RightsIssueId = r.RightsIssueId
			AND r.RightsIssueId = @RightsIssueId
		WHERE CustodianId = @CustodianId

		BEGIN TRANSACTION

	-- Delete Existing Rows
        DELETE FROM dbo.CustodianAcceptance
		FROM dbo.CustodianAcceptance ca
		JOIN #AcceptancesToDelete d
			ON ca.CustodianAcceptanceId = d.CustodianAcceptanceId

		-- insert records into Custodian Acceptances table for Custodian 
		select * from CustodianAcceptance
		INSERT INTO dbo.CustodianAcceptance
		(
			CustodianId
            ,CustodianInvestorId
            ,FullName
            ,Address1
            ,Address2
            ,Address3
            ,Address4
            ,Address5
            ,Address6
            ,Postcode
            ,CountryCode
            ,EmailAddress
            ,MobileNumber
            ,LastUpdatedAtUtc
			,TotalShares
			,InvestmentAmount
			,AdditionalShares
			,EntitledShares
			,OfferId

		)
		SELECT
            CustodianId
            ,CustodianInvestorId
            ,FullName
            ,Address1
            ,Address2
            ,Address3
            ,Address4
            ,Address5
            ,Address6
            ,Postcode
            ,CountryCode
            ,EmailAddress
            ,MobileNumber
            ,@v_LastUpdatedAtUtc
			,TotalShares
			,InvestmentAmount
			,AdditionalShares
			,EntitledShares
			,OfferId
		FROM @CustodianAcceptances
			
		COMMIT TRANSACTION

		RETURN (0)

	END TRY
	
	BEGIN CATCH
	
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

GRANT EXECUTE ON dbo.p_CreateCustodianAcceptances TO  [ServiceUsers];
GO