using ServiceAbstraction.Dtos;
using ServiceAbstraction.Dtos.BloodRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IAiMatchService
    {
        /// <summary>
        /// GET /api/ai/match-requests
        /// Loads the authenticated donor's profile, fetches filtered Open requests,
        /// recalculates priority, delegates ranking to the external AI service,
        /// merges requester names, and returns a user-friendly response.
        /// </summary>
        Task<FrontendMatchResponseDto> GetMatchedRequestsAsync(
            string userId,
            BloodRequestQueryParams queryParams);
    }
}
