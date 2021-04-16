IF  EXISTS (SELECT	1 
			FROM	dbo.sysobjects 
			WHERE	id = object_id(N'dbo.p_GetStatistics') 
				and objectproperty(id, N'IsProcedure') = 1
	)
	DROP PROCEDURE dbo.p_GetStatistics 
GO
 

CREATE PROCEDURE [dbo].[p_GetStatistics]
	@RightsIssueId integer
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

	DECLARE 
			 @TargetAmount DECIMAL(14,2),
			 @DaysToGo int,
			 @PricePerShare DECIMAL(10,4),
			 @TotalInvestmentAmount DECIMAL(14,2),
			 @TotalEntitledAmountAccepted DECIMAL(14,2),
			 @PercentageEntitledAmountAccepted DECIMAL(14,2),
			 @TotalAdditionalAmountAccepted DECIMAL(14,2),
			 @PercentageAdditionalAmountAccepted DECIMAL(14,2),
			 @TotalPaymentOutstanding   DECIMAL(14,2),
			 @TotalPaymentReceived  DECIMAL(14,2),
			 @TotalUnpaidAmount DECIMAL(14,2),
			 @TotalUnderpaidAmount DECIMAL(14,2),
			 @TotalReducedAmount DECIMAL(14,2),
			 @TotalPartialPaidAmount DECIMAL(14,2),
			 @TotalPaidAmountReceived DECIMAL(14,2),
			 @TotalCustodianPaidAmount DECIMAL(14,2),
			 @TotalAllocationRefunds DECIMAL(14,2),
			 @TotalAcceptanceRefunds DECIMAL(14,2),
			 @AmountToRefund   DECIMAL(14,2),
			 @TotalUnmatchedPayments DECIMAL(14,2),
			 @TotalUnmatchedPaymentCount int,
			 @PercentageTargetRaised   DECIMAL(14,2),
			 @TotalAmountAllocated DECIMAL(14,2)
			 
	  DECLARE @Acceptance TABLE 
			 (OfferId INT NOT NULL,
			 AcceptanceID INT NOT NULL,
			 TotalEntitledAmount DECIMAL (14,2) NOT NULL,
			 InvestmentAmount DECIMAL (14,2) NOT NULL,
			 Status varchar(20))

	  -- determine target amount and remaining days till close offer date
	  SELECT 
			 @TargetAmount = AmountToBeRaised
			 ,@DaysToGo =  DATEDIFF(day,  GetUTCdate(), OfferCloseDate) 
			 ,@PricePerShare = PricePerShare
	  FROM RightsIssue
	  WHERE RightsIssueId = @RightsIssueId

	  -- get all acceptance details and store in table variable to be used for gathering required totals
	  INSERT INTO @Acceptance
	  SELECT 
			 o.OfferId,
			 a.AcceptanceId,		
			 CEILING((o.EntitledShareBalance * @PricePerShare) * 100) / 100 as TotalEntitledAmount, 
			 a.InvestmentAmount,
			 a.Status
	  FROM dbo.Acceptance AS a
	  INNER JOIN dbo.Offer AS o
			 ON a.OfferId = o.OfferId 
	  WHERE o.RightsIssueId = @RightsIssueId
			 AND a.Status <> 'failed'
	  
	  -- treat all custodian acceptances as paid
	  -- Make Custodian acceptances negative to ensure they don't collide with retail acceptances
	  INSERT INTO @Acceptance
	  SELECT 
			 o.OfferId,
			 ca.CustodianAcceptanceId * -1, 
			 CEILING((ca.EntitledShares * @PricePerShare) * 100) / 100 as TotalEntitledAmount, 
			 ca.InvestmentAmount,
			 'paid'
	  FROM CustodianAcceptance AS ca
	  INNER JOIN dbo.Offer AS o
			 ON ca.OfferId = o.OfferId 
			 AND o.RightsIssueId = @RightsIssueId

	  -- calculate acceptance totals (exclude refunded and cancelled)
	  -- total investment amount
	  SELECT @TotalInvestmentAmount = SUM(InvestmentAmount)
	  FROM @Acceptance 
	  WHERE Status not in ('refunded', 'cancelled')

	  -- caculate total entitled and accepted amount 
	  SELECT 
			 @TotalEntitledAmountAccepted = SUM(EntitledAcceptedAmount),
			 @TotalAdditionalAmountAccepted = SUM(AdditionalAcceptedAmount)
	  FROM (
				   SELECT 
						  OfferId = o.OfferID,
						  EntitledAcceptedAmount = CASE WHEN SUM(a.InvestmentAmount) >  a.TotalEntitledAmount
															 THEN 
																	a.TotalEntitledAmount
													  ELSE 
															 SUM(a.InvestmentAmount) 
													  END,
						  AdditionalAcceptedAmount = CASE WHEN SUM(a.InvestmentAmount) >  a.TotalEntitledAmount
															 THEN 
																   SUM(a.InvestmentAmount) - a.TotalEntitledAmount
													  ELSE 
															 0
													  END 
				   FROM @Acceptance AS a
				   JOIN dbo.Offer AS o
				   ON o.OfferId = a.OfferId
				   WHERE o.RightsIssueId = @RightsIssueId
						  AND a.Status not in ('refunded', 'cancelled')
				   GROUP BY o.OfferId, a.TotalEntitledAmount
			 )  AS d

 	 -- calculate entitled and additional amount accepted in percentile
	  SELECT @PercentageEntitledAmountAccepted = CASE WHEN @TotalEntitledAmountAccepted > 0 
																				 THEN ROUND(CAST(@TotalEntitledAmountAccepted AS FLOAT) / CAST(@TotalInvestmentAmount AS FLOAT)  * 100.0, 2)
																				 ELSE 0.0              
																		  END

	  SELECT @PercentageAdditionalAmountAccepted =  CASE WHEN ISNULL(@TotalAdditionalAmountAccepted, 0) > 0.0 THEN 
																		  ROUND(CAST(@TotalAdditionalAmountAccepted AS FLOAT)  / CAST(@TotalInvestmentAmount AS FLOAT) * 100.0, 2)
																   ELSE 0.0
															 END


	  -- unpaid total investment amount
	  SELECT @TotalUnpaidAmount = ISNULL(SUM(a.InvestmentAmount), 0)
	  FROM @Acceptance AS a
	  WHERE a.Status in ('unpaid')

	  -- underpaid total investment amount 
	  SELECT @TotalUnderpaidAmount = ISNULL(SUM(a.InvestmentAmount), 0)
	  FROM @Acceptance AS a
	  WHERE a.Status in ('underpaid')

	  -- refunded total investment amount
	  SELECT @TotalReducedAmount = ISNULL(SUM(a.InvestmentAmount), 0)
	  FROM @Acceptance AS a
	  WHERE a.Status in ('reduced')

	  -- total Underpaid's partial paid amount
	  SELECT @TotalPartialPaidAmount = ISNULL(SUM(p.PaymentAmount), 0)
	  FROM @Acceptance AS a
	  INNER JOIN dbo.AcceptancePayment AS ap
			 ON ap.AcceptanceID = a.AcceptanceID
	  INNER JOIN dbo.Payment AS p
			 ON p.PaymentID = ap.PaymentID 
	  WHERE a.Status in ('underpaid')
			 AND p.PaymentType in ('eft', 'directdebit')
			 AND p.Status in ('success', 'matched')

	  -- calculate total custodian investment amount
	  SELECT @TotalCustodianPaidAmount = sum(InvestmentAmount)
	  FROM @Acceptance AS a
	  WHERE AcceptanceID < 0 -- negative id are custodian acceptances

	  -- calcualate total recieved amount
	  -- total fully paid amount
	  SELECT @TotalPaidAmountReceived = sum(AmountReceived) 
	  FROM @Acceptance AS a
	  INNER JOIN dbo.AcceptancePayment AS ap
			 ON ap.AcceptanceID = a.AcceptanceID
	  WHERE a.Status = 'paid'

	  -- calculate total outstanding (total unpaid amount + underpaid shortfall)
	  SELECT @TotalPaymentOutstanding    = ISNULL(@TotalUnpaidAmount,0) + (ISNULL(@TotalUnderpaidAmount, 0) - ISNULL(@TotalPartialPaidAmount, 0))

	  -- total amount received
	  SELECT @TotalPaymentReceived    = ISNULL(@TotalPaidAmountReceived, 0) +  
															 ISNULL(@TotalCustodianPaidAmount, 0) + 
															 ISNULL(@TotalPartialPaidAmount, 0) +
															 ISNULL(@TotalReducedAmount, 0)              

	  -- calculate total refunds
	  -- calculate total allocation refunds
	  SELECT @TotalAllocationRefunds = sum(a.RefundAmount)
	  FROM dbo.Offer AS o
	  INNER JOIN dbo.Allocation AS a
			 ON a.OfferId = o.OfferId 
	  WHERE o.RightsIssueId = @RightsIssueId

	  -- calculate total acceptance refunds
	  SELECT @TotalAcceptanceRefunds = sum(ar.RefundAmount)
	  FROM @Acceptance AS a
	  INNER JOIN dbo.AcceptanceRefund AS ar
			 ON ar.AcceptanceId = a.AcceptanceID

	  SELECT @AmountToRefund = ISNULL(@TotalAcceptanceRefunds,0)  +  ISNULL(@TotalAllocationRefunds , 0)

	  -- calculate total unmatched amount and count
	  SELECT @TotalUnmatchedPayments = sum(PaymentAmount),
			 @TotalUnmatchedPaymentCount = count(PaymentId)
	  FROM dbo.Payment 
	  WHERE Status = 'unmatched'
			 AND RightsIssueId = @RightsIssueId
			 AND PaymentType in ('eft', 'directdebit')

	  -- calcualte target raised in percetile
	  SELECT @PercentageTargetRaised = ROUND(@TotalPaymentReceived  / @TargetAmount * 100, 2)

	  -- calculate total  amount allocated
	  SELECT @TotalAmountAllocated = sum(a.AllocatedAmount)
	  FROM dbo.Offer AS o
	  INNER JOIN dbo.Allocation AS a
			 ON a.OfferId = o.OfferId 
	  WHERE o.RightsIssueId = @RightsIssueId



	  -- return final result
	  SELECT
			 TargetAmount = ISNULL(@TargetAmount, 0.0),
			 TotalElectedAmount = ISNULL(@TotalInvestmentAmount, 0.0),
			 DaysToGo = CASE WHEN @DaysToGo > 0 
											   THEN @DaysToGo
											   ELSE 0
										END,
			 TotalPaymentReceived = ISNULL(@TotalPaymentReceived,0.0),
			 TotalPaymentOutstanding = @TotalPaymentOutstanding, 
			 TotalEntitledAmountAccepted = ISNULL(@TotalEntitledAmountAccepted, 0.0),
			 PercentageEntitledAmountAccepted = @PercentageEntitledAmountAccepted, 
			 totalAdditionalAmountAccepted = ISNULL(@TotalAdditionalAmountAccepted, 0.0),
			 PercentageAdditionalAmountAccepted = @PercentageAdditionalAmountAccepted,
			 AmountToRefund = @AmountToRefund,
			 AmountRaised = ISNULL(@TotalPaymentReceived,0.0),
			 PercentageTargetRaised = ISNULL(@PercentageTargetRaised, 0.0),
			 TotalUnmatchedPayments = ISNULL(@TotalUnmatchedPayments, 0.0),
			 TotalUnmatchedPaymentCount  = ISNULL(@TotalUnmatchedPaymentCount, 0),
			 TotalAmountAllocated = ISNULL(@TotalAmountAllocated, 0)


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

GRANT EXECUTE ON dbo.p_GetStatistics TO  [ServiceUsers];
GO
