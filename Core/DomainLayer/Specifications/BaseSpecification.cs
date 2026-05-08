using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    // ── Interface ─────────────────────────────────────────────────────────────
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>>? Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDesc { get; }
        int? Take { get; }
        int? Skip { get; }
        bool IsPagingEnabled { get; }

        /// <summary>
        /// When true, EF Core applies AsNoTracking() — default for all read-only queries.
        /// </summary>
        bool IsReadOnly { get; }
    }

    // ── Base Implementation ───────────────────────────────────────────────────
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>>? Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDesc { get; private set; }
        public int? Take { get; private set; }
        public int? Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }

        /// <summary>
        /// Defaults to true — all reads are AsNoTracking unless explicitly overridden.
        /// </summary>
        public bool IsReadOnly { get; private set; } = true;

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
            => Includes.Add(includeExpression);

        protected void AddInclude(string includeString)
            => IncludeStrings.Add(includeString);

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpr)
            => OrderBy = orderByExpr;

        protected void ApplyOrderByDesc(Expression<Func<T, object>> orderByDescExpr)
            => OrderByDesc = orderByDescExpr;

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        /// <summary>
        /// Call this in write-path specs (GetById before Update/Delete)
        /// so EF Core tracks the entity and can persist changes.
        /// </summary>
        protected void DisableReadOnly() => IsReadOnly = false;
    }
}
