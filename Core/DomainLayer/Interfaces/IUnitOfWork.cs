using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<BloodRequest> BloodRequests { get; }
        IGenericRepository<ApplicationUser> Users { get; }
        IGenericRepository<Donation> Donations { get; }

        Task<int> SaveChangesAsync();
    }
}
