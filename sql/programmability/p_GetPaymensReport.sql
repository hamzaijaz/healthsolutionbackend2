IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_GetPaymentsReport') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_GetPaymentsReport
GO
 

CREATE PROCEDURE [dbo].[p_GetPaymentsReport]
	@RightsIssueKey UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
	

DECLARE @Payments TABLE
		(HolderId VARCHAR(20)  NOT NULL
		,FullName VARCHAR(200) NOT NULL
		,PostCode VARCHAR(200) NOT NULL
		,CountryCode VARCHAR(3) NOT NULL
		,EmailAddress VARCHAR(100) NOT NULL
		,MobileNumber VARCHAR(20) NOT NULL
		,BankCode  VARCHAR(20) NOT NULL
		,BankAccountNumber VARCHAR(20) NOT NULL
		,CustodianName VARCHAR(100) NOT NULL
		,CustodianInvestorId VARCHAR(20) NOT NULL
		,TotalAmountReceived DECIMAL(14,2) NOT NULL
		,PaymentReference VARCHAR(20) NOT NULL
		,DatePaymentReceived DATETIME NULL
		,PaymentDescription VARCHAR(100) NOT NULL
		,PaymentStatus VARCHAR(20) NOT NULL
		,PaymentMethod VARCHAR(20) NOT NULL
		,PaymentFailureReason VARCHAR(100) NOT NULL
		) 

		-- get RI Key based on input spid
		DECLARE @RightsIssueId INT
		
		SELECT @RightsIssueId  =   RightsIssueId FROM dbo.RightsIssue
		WHERE RightsIssueKey = @RightsIssueKey
			 
		-- extract retails payments
		INSERT INTO @Payments
		SELECT 
			HolderId
			,o.FullName
			,o.PostCode
			,o.CountryCode
			,o.EmailAddress
			,o.MobileNumber
			,BankCode = ISNULL(a.BankCode, '')
			,BankAccountNumber = ISNULL(a.BankAccountNumber, '')
			,CustodianName = ''
			,CustodianInvestorId = ''
			,TotalAmountReceived = CASE WHEN ISNULL(a.Status , '') = 'reduced' THEN a.InvestmentAmount
										ELSE ap.AmountReceived
									END
			,PaymentReference  = ISNULL(aer.ReferenceNumber, '')
			,DatePaymentReceived = CASE WHEN ISNULL(a.Status , '') = 'reduced' THEN CONVERT(datetime, SWITCHOFFSET(a.LastUpdatedAtUtc, DATEPART(TZOFFSET, a.LastUpdatedAtUtc AT TIME ZONE 'Eastern Standard Time')))
										WHEN ISNULL(a.Status , '') = 'refunded' THEN CONVERT(datetime, SWITCHOFFSET(ar.LastUpdatedAtUtc, DATEPART(TZOFFSET, ar.LastUpdatedAtUtc AT TIME ZONE 'Eastern Standard Time')))
										ELSE CONVERT(datetime, SWITCHOFFSET(p.PaymentDateAtUtc, DATEPART(TZOFFSET, p.PaymentDateAtUtc AT TIME ZONE 'Eastern Standard Time')))
									END
			,PaymentDescription = CASE WHEN ISNULL(a.Status , '') = 'reduced' THEN 'reduced'
										ELSE ISNULL(p.PaymentDescription, '')
									END
			,PaymentStatus = CASE WHEN ISNULL(ar.AcceptanceId, 0) > 0 THEN 'refunded'
								  WHEN p.Status = 'success' AND p.PaymentAmount > 0 THEN 'paid'
								  WHEN p.Status = 'success' AND p.PaymentAmount < 0 THEN 'refunded'
								  WHEN p.Status = 'failed' THEN 'failed'
								  WHEN p.Status = 'matched' THEN 'paid'
								  ELSE p.Status
								  END
			,PaymentMethod = ISNULL(a.PaymentMethod, '')
			,PaymentFailureReason = CASE WHEN ISNULL(a.Status , '') = 'failed' THEN p.StatusReason
										ELSE  ''
									END		 
		FROM dbo.Offer AS o
		JOIN  dbo.Acceptance AS a
			ON a.OfferId = o.OfferId
		JOIN dbo.AcceptancePayment AS ap
			ON ap.AcceptanceId = a.AcceptanceId
		JOIN dbo.Payment AS p
			ON p.PaymentId = ap.PaymentId
		LEFT JOIN  dbo.AcceptanceRefund AS ar
			ON a.AcceptanceId = ar.AcceptanceId
		LEFT JOIN dbo.AcceptanceEftReference AS aer
			ON aer.AcceptanceId = a.AcceptanceId
		LEFT JOIN dbo.CustodianOffer AS co
			ON co.OfferId = o.OfferId
		WHERE o.RightsIssueId = @RightsIssueId 
			AND ISNULL(co.CustodianId, 0 ) = 0
			AND (p.Status = 'success' OR p.Status = 'matched' OR p.Status = 'failed')
			

		-- extract custodian payments
		INSERT INTO @Payments
		SELECT 
			HolderId
			,o.FullName
			,o.PostCode
			,o.CountryCode
			,o.EmailAddress
			,o.MobileNumber
			,BankCode =  ''
			,BankAccountNumber = ''
			,CustodianName = c.Name
			,CustodianInvestorId = ca.CustodianInvestorId
			,TotalAmountReceived = ca.InvestmentAmount
			,PaymentReference  = ''
			,DatePaymentReceived = CONVERT(datetime, SWITCHOFFSET(ca.LastUpdatedAtUtc, DATEPART(TZOFFSET, ca.LastUpdatedAtUtc AT TIME ZONE 'Eastern Standard Time')))
			,PaymentDescription = ''
			,PaymentStatus = 'paid'
			,PaymentMethod = 'eft'
			,PaymentFailureReason = ''
		FROM dbo.Offer AS o
		JOIN dbo.CustodianOffer AS co
			ON co.OfferId = o.OfferId
		JOIN dbo.Custodian AS c
			ON c.CustodianId = co.CustodianId
		JOIN dbo.CustodianAcceptance AS ca
			ON ca.OfferId = o.OfferId
		WHERE o.RightsIssueId = @RightsIssueId 
		 
		-- return the combine retail and custodians payments
		SELECT
		HolderId 
		,FullName
		,PostCode
		,CountryCode
		,EmailAddress
		,MobileNumber
		,BankCode
		,BankAccountNumber
		,CustodianName
		,CustodianInvestorId
		,TotalAmountReceived
		,PaymentReference
		,DatePaymentReceived
		,PaymentDescription
		,PaymentStatus
		,PaymentMethod
		,PaymentFailureReason
		FROM @Payments
		ORDER BY HolderId, DatePaymentReceived

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

GRANT EXECUTE ON dbo.p_GetPaymentsReport TO  [ServiceUsers];
GO 