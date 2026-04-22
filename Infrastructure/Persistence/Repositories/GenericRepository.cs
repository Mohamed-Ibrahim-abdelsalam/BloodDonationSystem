using BloodDonationSystem.Data;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using Microsoft.EntityFrameworkCore;
using Persistence.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _ctx;

        public GenericRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<T?> GetByIdAsync(int id)
            => await _ctx.Set<T>().FindAsync(id);

        public async Task<T?> GetEntityWithSpecAsync(ISpecification<T> spec)
            => await SpecificationEvaluator<T>
                .GetQuery(_ctx.Set<T>().AsQueryable(), spec)
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec)
            => await SpecificationEvaluator<T>
                .GetQuery(_ctx.Set<T>().AsQueryable(), spec)
                .ToListAsync();

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _ctx.Set<T>().ToListAsync();

        public async Task AddAsync(T entity)
            => await _ctx.Set<T>().AddAsync(entity);

        public void Update(T entity)
            => _ctx.Set<T>().Update(entity);

        public void Delete(T entity)
            => _ctx.Set<T>().Remove(entity);
    }
}
