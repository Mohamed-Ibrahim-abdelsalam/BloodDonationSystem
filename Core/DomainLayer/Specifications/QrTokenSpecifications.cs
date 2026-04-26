using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    /// <summary>
    /// Find a QrToken by its token string — used in scan endpoints
    /// </summary>
    public class QrTokenByValueSpecification : BaseSpecification<QrToken>
    {
        public QrTokenByValueSpecification(string token)
        {
            AddInclude(q => q.Donation);
            AddInclude(q => q.BloodRequest);
            Criteria = q => q.Token == token;
        }
    }

    /// <summary>
    /// Find existing active QrToken for a donation (to prevent duplicates)
    /// </summary>
    public class ActiveDonationQrSpecification : BaseSpecification<QrToken>
    {
        public ActiveDonationQrSpecification(int donationId)
        {
            Criteria = q =>
                q.DonationId == donationId &&
                q.Type == QrTokenType.Donation &&
                !q.IsUsed &&
                q.ExpiryDate > DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Find existing active QrToken for a pickup request (to prevent duplicates)
    /// </summary>
    public class ActivePickupQrSpecification : BaseSpecification<QrToken>
    {
        public ActivePickupQrSpecification(int bloodRequestId)
        {
            Criteria = q =>
                q.BloodRequestId == bloodRequestId &&
                q.Type == QrTokenType.Pickup &&
                !q.IsUsed &&
                q.ExpiryDate > DateTime.UtcNow;
        }
    }
}
