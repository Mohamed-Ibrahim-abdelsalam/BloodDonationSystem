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
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            // Apply Where clause
            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            // Apply Includes (strongly-typed)
            query = spec.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply string Includes (e.g. nested: "RequestedByUser.Hospital")
            query = spec.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));

            // Apply ordering
            if (spec.OrderBy is not null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDesc is not null)
                query = query.OrderByDescending(spec.OrderByDesc);

            // Apply paging
            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip!.Value).Take(spec.Take!.Value);

            return query;
        }
    }
}
