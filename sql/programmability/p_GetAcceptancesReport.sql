IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_GetAcceptancesReport') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_GetAcceptancesReport
GO

CREATE PROCEDURE [dbo].[p_GetAcceptancesReport]
	@RightsIssueKey UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	

		DECLARE @AcceptancePayment TABLE
			(AcceptanceId INT NOT NULL
			,AmountReceived  DECIMAL(14,2) NOT NULL
			)

		DECLARE @AcceptanceRefund TABLE
			(AcceptanceId INT NOT NULL
			,RefundAmount  DECIMAL(14,2) NOT NULL
			)

		DECLARE @Acceptance TABLE
			(HolderId VARCHAR(20)  NOT NULL
			,FullName VARCHAR(200) NOT NULL
			,Address1 VARCHAR(200) NOT NULL
			,Address2 VARCHAR(200) NOT NULL
			,Address3 VARCHAR(200) NOT NULL
			,Address4 VARCHAR(200) NOT NULL
			,Address5 VARCHAR(200) NOT NULL
			,Address6 VARCHAR(200) NOT NULL
			,PostCode VARCHAR(200) NOT NULL
			,CountryCode VARCHAR(3) NOT NULL
			,EmailAddress VARCHAR(100) NOT NULL
			,MobileNumber VARCHAR(20) NOT NULL
			,BankCode  VARCHAR(20) NOT NULL
			,BankAccountNumber VARCHAR(20) NOT NULL
			,RecordDateBalance BIGINT NOT NULL
			,TotalSharesEntitled BIGINT NOT NULL
			,EntitledAmount DECIMAL(14,2) NOT NULL
			,AcceptanceAmount DECIMAL(14,2) NOT NULL
			,EntitledAmountPaid DECIMAL(14,2) NOT NULL
			,AdditionalAmountPaid DECIMAL(14,2) NOT NULL
			,TotalAmountPaid DECIMAL(14,2) NOT NULL
			,TotalSharesAllocated BIGINT NOT NULL
			,TotalAllocatedAmount DECIMAL(14,2) NOT NULL
			,TotalAllocationRefundAmount DECIMAL(14,2) NOT NULL
			,TotalAcceptanceRefundAmount DECIMAL(14,2) NOT NULL
			,ScalebackRatio DECIMAL(9, 8) NOT NULL
			) 

		DECLARE @Allocation TABLE(
			AllocationId INT NOT NULL,
			AllocationKey UNIQUEIDENTIFIER NOT NULL,
			OfferId INT NOT NULL,
			EntitledShares BIGINT NOT NULL,
			AdditionalShares BIGINT NOT NULL,
			AllocatedAmount DECIMAL (14, 2) NOT NULL,
			RefundAmount DECIMAL(14, 2) NOT NULL,
			ResidualAmount DECIMAL(14, 2) NOT NULL,
			ScalebackRatio DECIMAL(9, 8) NOT NULL,
			MinimumGuaranteedShares BIGINT NOT NULL,
			SharesRounding  NVARCHAR(10) NOT NULL,
			LastUpdatedAtUtc DATETIME NOT NULL
			)

		DECLARE @CustodianAllocation TABLE(
			CustodianAllocationId INT NOT NULL,
			CustodianAcceptanceId INT NOT NULL,
			AllocationID INT NOT NULL,
			EntitledShares BIGINT NOT NULL,
			AdditionalShares BIGINT NOT NULL,
			AllocatedAmount DECIMAL (14, 2) NOT NULL,
			RefundAmount DECIMAL(14, 2) NOT NULL,
			ResidualAmount DECIMAL(14, 2) NOT NULL,
			LastUpdatedAtUtc DATETIME NOT NULL
			)

		DECLARE @EntitledAndReceivedAmountForRetail TABLE
			(OfferId INT NOT NULL
			,HolderId VARCHAR(20)  NOT NULL
			,BankCode  VARCHAR(20) NOT NULL
			,BankAccountNumber VARCHAR(20) NOT NULL
			,EntitledAmount  DECIMAL(14,2) NOT NULL
			,AcceptanceAmount DECIMAL(14,2) NOT NULL
			,TotalAmountPaid DECIMAL(14,2) NOT NULL
			)

		DECLARE @EntitledAmountForCustodian TABLE
			(OfferId INT NOT NULL
			,EntitledAmount  DECIMAL(14,2) NOT NULL
			,AdditionalAmount DECIMAL(14,2) NOT NULL
			,TotalAmountPaid DECIMAL(14,2) NOT NULL
			,TotalAllocatedShares BIGINT NOT NULL
			,TotalAllocatedAmount DECIMAL(14,2) NOT NULL
			,TotalAllocationRefundAmount DECIMAL(14,2) NOT NULL
			)

 		-- get RI Key based on input spid
		DECLARE @RightsIssueId INT

		DECLARE @PricePerShare DECIMAL(10,4)

		SELECT @RightsIssueId  =  RightsIssueId 
				,@PricePershare = PricePershare
		FROM dbo.RightsIssue
		WHERE RightsIssueKey = @RightsIssueKey
			 
		DECLARE @v_TableAllocationStaging VARCHAR(255) = 'Allocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')
		DECLARE @sql1 NVARCHAR(MAX) = 'SELECT * FROM ' + @v_TableAllocationStaging

		DECLARE @v_TableCustodianAllocationStaging VARCHAR(255) = 'CustodianAllocation_' + REPLACE(CONVERT(varchar(36), @RightsIssueKey),'-','_')
		DECLARE @sql2 NVARCHAR(MAX) = 'SELECT * FROM ' + @v_TableCustodianAllocationStaging

		-- if allocation staging table then load to a table variable
		IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_NAME = @v_TableAllocationStaging)
		BEGIN
			INSERT INTO @Allocation
			EXEC sp_executesql @sql1

			INSERT INTO @CustodianAllocation
			EXEC sp_executesql @sql2
		END
		ELSE
		BEGIN
			INSERT INTO @Allocation
			SELECT a.* FROM dbo.Allocation AS a
			JOIN dbo.Offer AS o
				ON o.OfferId = a.OfferId 
			WHERE o.RightsIssueId = @RightsIssueId

			INSERT INTO @CustodianAllocation
			SELECT ca.* 
			FROM dbo.CustodianAllocation AS ca
			JOIN dbo.Allocation AS a
				ON a.AllocationId = ca.AllocationId
			JOIN dbo.Offer AS o
				ON o.OfferId = a.OfferId 
			WHERE o.RightsIssueId = @RightsIssueId

		END
 
		-- Get Acceptance total payment received
		INSERT INTO @AcceptancePayment
		SELECT 
			a.AcceptanceId
			,SUM(ap.AmountReceived)
		FROM dbo.Offer AS o
		JOIN  dbo.Acceptance AS a
			ON a.OfferId = o.OfferId
		JOIN dbo.AcceptancePayment AS ap
			ON ap.AcceptanceId = a.AcceptanceId
		JOIN dbo.Payment AS p
			ON p.PaymentId = ap.PaymentId
		WHERE o.RightsIssueId = @RightsIssueId
			AND p.Status in ('matched', 'success')
		GROUP BY a.AcceptanceId

		-- Get Acceptance refund 
		INSERT INTO @AcceptanceRefund
		SELECT 
			a.AcceptanceId
			,ar.RefundAmount
		FROM dbo.Offer AS o
		JOIN  dbo.Acceptance AS a
			ON a.OfferId = o.OfferId
		JOIN  dbo.AcceptanceRefund AS ar
			ON ar.AcceptanceId = a.AcceptanceId
		WHERE o.RightsIssueId = @RightsIssueId
	 
		-- Determine Entitled Amount and Total Amount Received
		INSERT INTO @EntitledAndReceivedAmountForRetail
		SELECT 
			o.OfferId
			,o.HolderId
			,d.BankCode
			,d.BankAccountNumber
			,EntitledAmount = CEILING((o.EntitledShareBalance * @PricePerShare) * 100) / 100 -- round up
			,AcceptanceAmount = SUM(CASE WHEN NOT a.Status in ('refunded', 'reduced', 'cancelled') THEN a.InvestmentAmount
											ELSE 0 END) 
			,TotalAmountPaid = SUM(CASE WHEN NOT a.Status in ('refunded', 'reduced', 'cancelled') THEN ISNULL (ap.AmountReceived, 0)
											WHEN a.Status in ('reduced') THEN a.InvestmentAmount
											ELSE 0 END) 
		FROM dbo.Offer AS o 
		JOIN  dbo.Acceptance AS a
			ON a.OfferId = o.OfferId
		JOIN (SELECT OfferID,
                ROW_NUMBER() OVER(PARTITION BY OfferId ORDER BY DateAcceptedAtUtc DESC) AS RowNumber
                ,BankCode
                ,BankAccountNumber
                ,DateAcceptedAtUtc
                ,PaymentMethod
                FROM Acceptance) AS d
			ON d.OfferId = a.OfferId
				AND RightsIssueId = @RightsIssueId
				AND d.RowNumber = 1
		LEFT JOIN @AcceptancePayment AS ap
			ON ap.AcceptanceId = a.AcceptanceId
		WHERE NOT ISNULL(a.Status, '')  in ('failed')
		AND o.RightsIssueId = @RightsIssueId 
		GROUP BY 
			o.OfferID
			,o.HolderId
			,o.EntitledShareBalance
			,d.BankCode
			,d.BankAccountNumber

		-- Populate this table with all of the individual Custodian Acceptance and Allocation details
		INSERT INTO @EntitledAmountForCustodian
		SELECT
			o.OfferId
			, CEILING((ca.EntitledShares * @PricePerShare) * 100) / 100 -- round up
			, ca.InvestmentAmount - CEILING((ca.EntitledShares * @PricePerShare) * 100) / 100 -- round up
			, ca.InvestmentAmount
			,TotalAllocatedShares = ISNULL(cal.EntitledShares, 0) + ISNULL(cal.AdditionalShares, 0)
			,TotalAllocatedAmount = ISNULL(cal.AllocatedAmount, 0)
			,TotalAllocationRefundAmount = ISNULL(cal.RefundAmount, 0)
		FROM dbo.Offer AS o
		JOIN dbo.CustodianOffer AS co 
			ON co.OfferId = o.OfferId
		JOIN  dbo.CustodianAcceptance AS ca
            ON ca.OfferId = co.OfferId
		JOIN  @CustodianAllocation AS cal
            ON ca.CustodianAcceptanceId = cal.CustodianAcceptanceId
		WHERE o.RightsIssueId = @RightsIssueId
		GROUP BY
			o.OfferId
            ,ca.EntitledShares
			,ca.InvestmentAmount
			,ca.EntitledShares
			,cal.EntitledShares
			,cal.AdditionalShares
			,cal.AllocatedAmount
			,cal.RefundAmount

		-- Retails
		INSERT INTO @Acceptance
		SELECT 
			o.HolderId
			,o.FullName
			,o.Address1
			,o.Address2
			,o.Address3
			,o.Address4
			,o.Address5
			,o.Address6
			,o.PostCode
			,o.CountryCode
			,o.EmailAddress
			,o.MobileNumber
			,era.BankCode
			,era.BankAccountNumber
			,o.ShareBalance				
			,o.EntitledShareBalance
			,era.EntitledAmount
			,era.AcceptanceAmount 
			,EntitledAmountPaid = CASE WHEN era.TotalAmountPaid >= era.EntitledAmount 
									   THEN era.EntitledAmount 
								  ELSE era.TotalAmountPaid
								  END
			,AdditionalAmountPaid = CASE WHEN era.TotalAmountPaid >= era.EntitledAmount 
										 THEN era.TotalAmountPaid - era.EntitledAmount
									ELSE 0
									END
			,era.TotalAmountPaid
			,TotalSharesAllocated = ISNULL(al.EntitledShares, 0) + ISNULL(al.AdditionalShares, 0)   
			,TotalAllocatedAmount =ISNULL(al.AllocatedAmount, 0)  
			,TotalAllocationRefundAmount = MAX(ISNULL(al.RefundAmount, 0)) 
			,TotalAcceptanceRefundAmount = SUM(ISNULL(ar.RefundAmount, 0))  
			,ISNULL(al.ScalebackRatio, 0)

		FROM dbo.Offer AS o 
		JOIN  dbo.Acceptance AS a
			ON a.OfferId = o.OfferId
		JOIN  @EntitledAndReceivedAmountForRetail AS era
			ON era.OfferId = o.OfferId
			AND era.HolderID = o.HolderId
		LEFT JOIN  @AcceptanceRefund AS ar
			ON a.AcceptanceId = ar.AcceptanceId
		LEFT JOIN @Allocation AS al
			ON al.OfferId= o.OfferId
		LEFT JOIN dbo.AllocationRefund AS alr
			ON alr.AllocationId = al.AllocationId
		LEFT JOIN @AcceptancePayment AS ap
			ON ap.AcceptanceId = a.AcceptanceId
		WHERE NOT ISNULL(a.Status, '')  in ('failed')
		AND o.RightsIssueId = @RightsIssueId 
		GROUP BY 
			o.HolderId
			,o.FullName
			,o.Address1
			,o.Address2
			,o.Address3
			,o.Address4
			,o.Address5
			,o.Address6
			,o.PostCode
			,o.CountryCode
			,o.EmailAddress
			,o.MobileNumber
			,o.ShareBalance
			,o.EntitledShareBalance
			,era.EntitledAmount
			,era.AcceptanceAmount 
			,era.TotalAmountPaid
			,era.BankCode
			,era.BankAccountNumber
			,al.EntitledShares
			,al.AdditionalShares
			,al.AllocatedAmount
			,al.ScalebackRatio
			
			-- Custodians
		INSERT INTO @Acceptance
		SELECT 
			HolderId
			,o.FullName
			,o.Address1
			,o.Address2
			,o.Address3
			,o.Address4
			,o.Address5
			,o.Address6
			,o.PostCode
			,o.CountryCode
			,o.EmailAddress
			,o.MobileNumber
			,BankCode  = ''
			,BankAccountNumber  = ''
			,o.ShareBalance				
			,o.EntitledShareBalance
			,SUM(eac.EntitledAmount)
			,AcceptanceAmount = SUM(eac.TotalAmountPaid)
			,EntitledAmountPaid = SUM(eac.EntitledAmount)
			,AdditionalAmountPaid = SUM(eac.AdditionalAmount)
			,TotalAmountPaid = SUM(eac.TotalAmountPaid)
			,TotalAllocatedShares = SUM(eac.TotalAllocatedShares)
			,TotalAllocatedAmount = SUM(eac.TotalAllocatedAmount)
			,TotalAllocationRefundAmount =  SUM(TotalAllocationRefundAmount)
			,TotalAcceptanceRefundAmount = 0
			,ISNULL(al.ScalebackRatio, 0)
		FROM dbo.Offer AS o
		JOIN dbo.CustodianOffer AS co 
			ON co.OfferId = o.OfferId
		JOIN @EntitledAmountForCustodian AS eac
			ON eac.OfferId = o.OfferId
		LEFT JOIN @Allocation AS al
			ON al.OfferId= o.OfferId
		WHERE o.RightsIssueId = @RightsIssueId
		GROUP BY 
			o.HolderId
            ,o.FullName
            ,o.Address1
            ,o.Address2
            ,o.Address3
            ,o.Address4
            ,o.Address5
            ,o.Address6
            ,o.PostCode
            ,o.CountryCode
            ,o.EmailAddress
            ,o.MobileNumber
            ,o.ShareBalance
            ,o.EntitledShareBalance
            ,al.ScalebackRatio

		-- return final result
		SELECT
			HolderId
			,FullName
			,Address1
			,Address2
			,Address3
			,Address4
			,Address5
			,Address6
			,PostCode
			,CountryCode
			,EmailAddress
			,MobileNumber
			,BankCode 
			,BankAccountNumber
			,RecordDateBalance
			,TotalSharesEntitled
			,EntitledAmount
			,AcceptanceAmount
			,EntitledAmountPaid
			,AdditionalAmountPaid
			,TotalAmountPaid = CASE WHEN TotalAmountPaid = TotalAcceptanceRefundAmount  -- means fully refunded
									THEN 0 ELSE TotalAmountPaid
							   END
			,TotalSharesAllocated
			,TotalAllocatedAmount = CASE WHEN TotalAmountPaid = TotalAcceptanceRefundAmount -- means fully refunded
										 THEN 0 ELSE TotalAllocatedAmount 
									END
			,TotalRefundAmount = TotalAcceptanceRefundAmount + TotalAllocationRefundAmount 
			,ScalebackRatio
		FROM @Acceptance
 
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

GRANT EXECUTE ON dbo.p_GetAcceptancesReport TO  [ServiceUsers];
GO 

  