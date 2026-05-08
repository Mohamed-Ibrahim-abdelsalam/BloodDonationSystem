using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ── Input ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Query-string parameters sent by the client for any paginated endpoint.
    /// Bound via [FromQuery] in the controller — no business logic here.
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 10;
        private const int DefaultPageSize = 5;

        private int _pageSize = DefaultPageSize;
        private int _pageNumber = 1;

        /// <summary>Minimum value 1. Values below 1 are clamped to 1.</summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Maximum value 10, default 5.
        /// Values above 10 are clamped to 10; values below 1 fall back to default (5).
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value <= 0)
                    _pageSize = DefaultPageSize;
                else if (value > MaxPageSize)
                    _pageSize = MaxPageSize;
                else
                    _pageSize = value;
            }
        }
    }

    // ── Output ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generic paginated wrapper returned by every paginated endpoint.
    /// <typeparamref name="T"/> is the DTO type for the items in the current page.
    /// </summary>
    public class PaginatedResponse<T>
    {
        public int CurrentPage { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public IEnumerable<T> Data { get; init; } = Enumerable.Empty<T>();

        /// <summary>
        /// Factory — calculates TotalPages from TotalCount and PageSize automatically.
        /// </summary>
        public static PaginatedResponse<T> Create(
            IEnumerable<T> data,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var totalPages = pageSize > 0
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0;

            return new PaginatedResponse<T>
            {
                Data = data,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }
    }
}
