IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_GetCustodianAcceptances') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_GetCustodianAcceptances  
GO
 

CREATE PROCEDURE [dbo].[p_GetCustodianAcceptances]
    @RightsIssueKey UNIQUEIDENTIFIER,
    @RightsIssueId INT,
    @CustodianId INT

AS
BEGIN
	SET NOCOUNT ON;
	
	-- build staging table name by using the share purchase plan key (GUID).
    DECLARE @v_TableAllocationStaging VARCHAR(255) = 'Allocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')
    DECLARE @v_TableCustodianAllocationStaging VARCHAR(255) = 'CustodianAllocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')

	BEGIN TRY

		IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_NAME = @v_TableAllocationStaging)
			BEGIN

			DECLARE @sql nvarchar(max);

			SET @sql = 'SELECT 
				c.CustodianKey			AS CustodianId
				,i.Name					AS CompanyName
				,i.TickerCode			AS Ticker
				,''RightsIssue''		AS EventName
				,ri.PricePerShare		AS PricePerShare
				,o.HolderId				
				,o.OfferKey				AS OfferId
				, CASE WHEN rtrim(CustodianInvestorId) = ''''
						THEN o.EntitledShareBalance	
					ELSE 
						ca.EntitledShares
				END AS TotalSharesEntitled
				, CASE WHEN rtrim(CustodianInvestorId) = ''''
						THEN CONVERT(DECIMAL(14,2), CEILING((o.EntitledShareBalance * ri.PricePerShare) * 100) / 100)
					ELSE
						CONVERT(DECIMAL(14,2), CEILING((ca.EntitledShares * ri.PricePerShare) * 100) / 100)
				END AS TotalEntitledAmountToPay 
				,ca.CustodianInvestorId
				,ca.FullName
				,ca.Address1
				,ca.Address2
				,ca.Address3
				,ca.Address4
				,ca.Address5
				,ca.Address6
				,ca.Postcode
				,ca.CountryCode
				,ca.EmailAddress
				,ca.MobileNumber
				,ca.EntitledShares					AS EntitledSharesAccepted
				,ca.AdditionalShares				AS AdditionalSharesAccepted
				,ca.TotalShares						AS TotalSharesAccepted
				,ca.InvestmentAmount				AS TotalAmountAccepted
				,ISNULL(cal.EntitledShares, 0) + ISNULL(cal.AdditionalShares, 0)	AS SharesAllocated
				,ISNULL(cal.RefundAmount, 0.0)		AS AmountToRefund
				,ISNULL(a.ScalebackRatio, 0.0)		AS ScalebackRatio
			FROM dbo.Issuer AS i
			INNER JOIN dbo.RightsIssue AS ri
				ON ri.IssuerId = i.IssuerId
			INNER JOIN dbo.Offer AS o
				ON o.RightsIssueId = ri.RightsIssueId
			INNER JOIN dbo.CustodianAcceptance AS ca
				ON ca.OfferId = o.OfferId
			INNER JOIN dbo.Custodian AS c
				ON c.CustodianId = ca.CustodianId
			LEFT OUTER JOIN ' + @v_TableCustodianAllocationStaging + ' AS cal
				ON cal.CustodianAcceptanceId = ca.CustodianAcceptanceId
			LEFT OUTER JOIN ' +  @v_TableAllocationStaging + ' AS a 
				ON a.AllocationId = cal.AllocationId'  +  
			' WHERE ri.RightsIssueId  = ' +  CONVERT(nvarchar(10), @RightsIssueId) + 
			' AND c.CustodianId = ' +  CONVERT(nvarchar(10), @CustodianId)
			
			EXEC sp_executesql @sql 



			END			
		ELSE
			BEGIN
			SELECT 
				c.CustodianKey			AS CustodianId
				,i.Name					AS CompanyName
				,i.TickerCode			AS Ticker
				,'RightsIssue'			AS EventName
				,ri.PricePerShare		AS PricePerShare
				,o.HolderId				
				,o.OfferKey				AS OfferId
				, CASE WHEN rtrim(CustodianInvestorId) = '' 
						THEN o.EntitledShareBalance	
					ELSE
						ca.EntitledShares
				END AS TotalSharesEntitled
				, CASE WHEN rtrim(CustodianInvestorId) = '' 
						THEN CEILING((o.EntitledShareBalance * ri.PricePerShare) * 100) / 100
					ELSE
						CEILING((ca.EntitledShares * ri.PricePerShare) * 100) / 100
				END AS TotalEntitledAmountToPay 
				,ca.CustodianInvestorId
				,ca.FullName
				,ca.Address1
				,ca.Address2
				,ca.Address3
				,ca.Address4
				,ca.Address5
				,ca.Address6
				,ca.Postcode
				,ca.CountryCode
				,ca.EmailAddress
				,ca.MobileNumber
				,ca.EntitledShares					AS EntitledSharesAccepted
				,ca.AdditionalShares				AS AdditionalSharesAccepted
				,ca.TotalShares						AS TotalSharesAccepted
				,ca.InvestmentAmount				AS TotalAmountAccepted
				,ISNULL(cal.EntitledShares, 0) + ISNULL(cal.AdditionalShares, 0)	AS SharesAllocated
				,ISNULL(cal.RefundAmount, 0.0)		AS AmountToRefund
				,ISNULL(a.ScalebackRatio, 0.0)		AS ScalebackRatio
			FROM dbo.Issuer AS i
			INNER JOIN dbo.RightsIssue AS ri
				ON ri.IssuerId = i.IssuerId
			INNER JOIN dbo.Offer AS o
				ON o.RightsIssueId = ri.RightsIssueId
			INNER JOIN dbo.CustodianAcceptance AS ca
				ON ca.OfferId = o.OfferId
			INNER JOIN dbo.Custodian AS c
				ON c.CustodianId = ca.CustodianId
			LEFT OUTER JOIN dbo.CustodianAllocation AS cal
				ON cal.CustodianAcceptanceId = ca.CustodianAcceptanceId
			LEFT OUTER JOIN dbo.Allocation AS a
				ON a.AllocationId = cal.AllocationId
			WHERE ri.RightsIssueId = @RightsIssueId
				AND c.CustodianId = @CustodianId

			END
			
	RETURN (0)

	END TRY
	
	BEGIN CATCH
	
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

GRANT EXECUTE ON dbo.p_GetCustodianAcceptances TO  [ServiceUsers];
GO 
 