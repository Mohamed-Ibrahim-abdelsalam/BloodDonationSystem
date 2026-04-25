using BloodDonationSystem.Data;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _ctx;

        private IGenericRepository<BloodRequest>? _bloodRequests;
        private IGenericRepository<ApplicationUser>? _users;
        private IGenericRepository<Donation>? _donations;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<Reward>? _rewards;
        private IGenericRepository<UserReward>? _userRewards;

        public UnitOfWork(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public IGenericRepository<BloodRequest> BloodRequests
            => _bloodRequests ??= new GenericRepository<BloodRequest>(_ctx);

        public IGenericRepository<ApplicationUser> Users
            => _users ??= new GenericRepository<ApplicationUser>(_ctx);

        public IGenericRepository<Donation> Donations
            => _donations ??= new GenericRepository<Donation>(_ctx);

        public IGenericRepository<Notification> Notifications
            => _notifications ??= new GenericRepository<Notification>(_ctx);

        public IGenericRepository<Reward> Rewards
            => _rewards ??= new GenericRepository<Reward>(_ctx);

        public IGenericRepository<UserReward> UserRewards
            => _userRewards ??= new GenericRepository<UserReward>(_ctx);

        public async Task<int> SaveChangesAsync()
            => await _ctx.SaveChangesAsync();

        public void Dispose()
            => _ctx.Dispose();
    }
}
