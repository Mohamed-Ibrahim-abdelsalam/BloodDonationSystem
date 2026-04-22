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
    }
}
