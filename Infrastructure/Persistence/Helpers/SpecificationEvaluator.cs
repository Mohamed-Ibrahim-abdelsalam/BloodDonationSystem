using DomainLayer.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Helpers
{
    public static class SpecificationEvaluator<T> where T : class
    {
        /// <summary>
        /// Builds the full IQueryable: filter → includes → order → paging.
        /// Applies AsNoTracking() when spec.IsReadOnly is true.
        /// </summary>
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            // AsNoTracking for all read-only specs — no change tracking overhead
            if (spec.IsReadOnly)
                query = query.AsNoTracking();

            // Apply Where clause
            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            // Apply strongly-typed Includes
            query = spec.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply string-based Includes (nested: "RequestedByUser.Hospital")
            query = spec.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply ordering
            if (spec.OrderBy is not null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDesc is not null)
                query = query.OrderByDescending(spec.OrderByDesc);

            // Apply paging (ALWAYS last — after filter and order)
            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip!.Value).Take(spec.Take!.Value);

            return query;
        }

        /// <summary>
        /// Builds a lightweight IQueryable for COUNT only:
        /// applies filter criteria — skips includes, ordering, and paging.
        /// Used to get TotalCount for pagination metadata.
        /// </summary>
        public static IQueryable<T> GetCountQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery.AsNoTracking();

            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            return query;
        }
    }
}
