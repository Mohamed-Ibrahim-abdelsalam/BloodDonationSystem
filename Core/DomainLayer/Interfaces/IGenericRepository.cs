using DomainLayer.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetEntityWithSpecAsync(ISpecification<T> spec);
        Task<T?> GetEntityWithSpecAsNoTrackingAsync(ISpecification<T> spec);
        Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec);
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Returns the count of entities matching the spec's Criteria only —
        /// no includes, no ordering, no paging. Used for pagination metadata.
        /// </summary>
        Task<int> CountAsync(ISpecification<T> spec);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
