using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IBloodPredictionService
    {
        /// <summary>
        /// GET /api/hospital/predictions?horizonDays=7|14|30
        /// Loads hospital blood bags, sends to Python prediction service,
        /// and returns a clean dashboard-ready response.
        /// </summary>
        Task<FrontendPredictionResponseDto> GetPredictionsAsync(
            string userId,
            int horizonDays);
    }
}
