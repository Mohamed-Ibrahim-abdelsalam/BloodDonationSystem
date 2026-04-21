using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(ApplicationUser user, string role);
        string GenerateRefreshToken();
        int GetAccessTokenExpirySeconds();
    }
}
