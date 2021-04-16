using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using EFCore.BulkExtensions;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using System;
using System.Linq;
using System.Data;
using System.ComponentModel;
using CapitalRaising.RightsIssues.Service.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using CapitalRaising.RightsIssues.Service.Domain.CustomEntities;

namespace CapitalRaising.RightsIssues.Service.Infrastructure.Persistence
{
    public partial class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDateTime _dateTime;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTime dateTime) : base(options)
        {
            _dateTime = dateTime;
        }

        public virtual DbSet<Statistic> Statistics { get; set; }

        public virtual DbSet<CustodianAcceptanceDetail> CustodianAcceptanceDetails { get; set; }

        public virtual DbSet<OfferAcceptanceRecord> OfferAcceptanceRecords { get; set; }
        public virtual DbSet<OfferPaymentRecord> OfferPaymentRecords { get; set; }

        public async Task BulkInsertAsync<T>(IList<T> entities, CancellationToken cancellationToken = default) where T : class
        {
            // inmemory does not allow bulk insert
            if (this.Database.IsInMemory())
            {
                Set<T>().AddRange(entities);

                await SaveChangesAsync(cancellationToken);
            }
            else
            {
                await DbContextBulkExtensions.BulkInsertAsync<T>(this, entities);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // before we save changes, automatically set the LastUpdatedAtUtc column
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                entry.Metadata.FindDeclaredProperty("LastUpdatedAtUtc")?.PropertyInfo.SetValue(entry.Entity, _dateTime.UtcNow);
            }
            
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Detect duplicate key errors.
                if(ex.InnerException != null && ex.InnerException.Message.Contains("Cannot insert duplicate key"))
                {
                    var r = new Regex(@"(dbo.)(\*|\w+)",
                        RegexOptions.IgnoreCase|RegexOptions.Compiled);
                    
                    Match m = r.Match(ex.InnerException.Message);
                    if(m.Success && m.Groups.Count > 1)
                    {
                        throw new DuplicateItemException(m.Groups[2].Value);
                    }
                    else
                    {
                        throw new DuplicateItemException(ChangeTracker.Entries().First().Entity.GetType().Name);
                    }
                }
                throw;
            }
            
        }
        
        private DataTable ToDataTable<T>(IEnumerable<T> data) where T : class
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}