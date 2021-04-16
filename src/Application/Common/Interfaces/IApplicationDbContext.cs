using System.Collections.Generic;
using Entities = CapitalRaising.RightsIssues.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Domain.Entities;
using CapitalRaising.RightsIssues.Service.Domain.CustomEntities;
using System;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Entities.Patient> Patients { get; set; }

        //DbSet<Entities.EventHistory> EventHistories { get; set; }
        
        //DbSet<Entities.Issuer> Issuers { get; set; }

        //DbSet<Entities.RightsIssue> RightsIssues { get; set; }

        //DbSet<Entities.Offer> Offers { get; set; }
        
        //DbSet<Entities.Acceptance> Acceptances { get; set; }

        //DbSet<Entities.AcceptanceRefund> AcceptanceRefunds { get; set; }

        //DbSet<Entities.Custodian> Custodians{ get; set; }

        //DbSet<Entities.CustodianAccess> CustodianAccesses { get; set; }

        //DbSet<CustodianAcceptance> CustodianAcceptances { get; set; }
        
        //DbSet<CustodianOffer> CustodianOffers { get; set; }

        //DbSet<Payment> Payments { get; set; }

        //DbSet<SettlementBankAccount> SettlementBankAccounts { get; set; }

        //DbSet<AcceptanceEftReference> AcceptanceEftReferences { get; set; }

        //DbSet<AcceptancePayment> AcceptancePayments { get; set; }

        //DbSet<AllocationRefund> AllocationRefunds { get; set; }

        //DbSet<Allocation> Allocations { get; set; }

        //DbSet<IssuerEmailAddress> IssuerEmailAddresses { get; set; }

        //DbSet<ControlFlag> ControlFlags { get; set; }

        //Task BulkInsertAsync<T>(IList<T> entities, CancellationToken cancellationToken = default) where T : class;

        //Task BulkInsertOffers(RightsIssue rightsIssue, IList<Offer> offers, CancellationToken cancellationToken = default);

        //Task CreateAllocations(RightsIssue rightsIssue, IList<Allocation> allocations, CancellationToken cancellationToken = default);
        
        //Task ConfirmAllocations(RightsIssue rightsIssue, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        //Task<Statistic> GetStatistics(int sharePurchasePlanId, CancellationToken cancellationToken = default);

        //Task BulkInsertCustodianAcceptances(RightsIssue rightsIssue, Custodian custodian, IList<CustodianAcceptance> custodianAcceptances, CancellationToken cancellationToken = default);

        //Task<List<CustodianAcceptanceDetail>> GetCustodianAcceptances(Guid rightsIssueKey, int rightsIssueId, int custodianId, CancellationToken cancellationToken = default);

        //Task<List<OfferAcceptanceRecord>> GetAcceptancesReport(Guid rightsIssueKey, CancellationToken cancellationToken = default);

        //Task<List<OfferPaymentRecord>> GetPaymentsReport(Guid rightsIssueKey, CancellationToken cancellationToken = default);
    }
}
