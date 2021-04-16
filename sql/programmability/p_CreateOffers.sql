IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_CreateOffers') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_CreateOffers
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
		and t.name = 't_Offers')

	drop type dbo.t_Offers

go

/****** Object:  UserDefinedTableType [dbo].[t_Offer]    Script Date: 18/06/2020 11:39:47 AM ******/
CREATE TYPE [dbo].[t_Offers] AS TABLE(
	[OfferKey] [uniqueidentifier] NOT NULL,
	[RightsIssueId] [int] NOT NULL,
	[HolderId] [varchar](20) NOT NULL,
	[ShareBalance] [bigint] NOT NULL,
	[EntitledShareBalance] [bigint] NOT NULL,
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
	[CustodianId] [int] NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.t_Offers TO  [ServiceUsers];
GO

GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE [dbo].[p_CreateOffers]
	@RightsIssueId int,
	@Offers dbo.t_Offers READONLY
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		DECLARE @v_LastUpdateAtUtc DateTime = GetUTCDate();

		BEGIN TRANSACTION

		-- Cleanup existing data
		DELETE FROM dbo.CustodianOffer
		FROM dbo.CustodianOffer AS co
		JOIN dbo.Offer AS o
			ON o.OfferId = co.OfferId
			AND o.RightsIssueId = @RightsIssueId
		
		DELETE FROM dbo.Offer
		WHERE RightsIssueId = @RightsIssueId
		
		INSERT dbo.Offer
		(
			OfferKey
			,RightsIssueId
			,HolderId
			,ShareBalance
			,EntitledShareBalance
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
		)
		SELECT
			OfferKey
			,RightsIssueId
			,HolderId
			,ShareBalance
			,EntitledShareBalance
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
			,@v_LastUpdateAtUtc
		FROM @Offers

		-- insert records into Custodian Offers table for Custodian 
		INSERT INTO dbo.CustodianOffer
		(
			CustodianId
			,OfferId
			,LastUpdatedAtUtc
		)
		SELECT
			d.CustodianId
			,o.OfferId
			,@v_LastUpdateAtUtc
		FROM @Offers AS d
		JOIN dbo.Offer AS o
			ON o.OfferKey = d.OfferKey
		WHERE d.CustodianId IS NOT NULL
			
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

GRANT EXECUTE ON dbo.p_CreateOffers TO  [ServiceUsers];
GO