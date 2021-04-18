using Entities = MyHealthSolution.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Entities.Patient> Patients { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
