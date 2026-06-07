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
            DisableReadOnly(); // tracked — IsUsed is set to true after scan
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



    /// <summary>
    /// Find an active (not used, not expired) Pickup QR linked to a specific Donation.
    /// Used when generating withdrawal QR for general donations.
    /// </summary>
    public class ActiveDonationPickupQrSpecification : BaseSpecification<QrToken>
    {
        public ActiveDonationPickupQrSpecification(int donationId)
        {
            Criteria = t =>
                t.DonationId == donationId &&
                t.Type == QrTokenType.Pickup &&
                !t.IsUsed &&
                t.ExpiryDate > DateTime.UtcNow;
        }
    }



    /// <summary>
    /// Find an active (not used, not expired) Reward QR linked to a UserReward.
    /// Used when generating reward QR to return existing valid token.
    /// </summary>
    public class ActiveRewardQrSpecification : BaseSpecification<QrToken>
    {
        public ActiveRewardQrSpecification(int userRewardId)
        {
            Criteria = t =>
                t.UserRewardId == userRewardId &&
                t.Type == QrTokenType.Reward &&
                !t.IsUsed &&
                t.ExpiryDate > DateTime.UtcNow;
        }
    }


    /// <summary>
    /// Finds ANY Pickup QrToken linked to a BloodRequest — regardless of IsUsed or expiry.
    /// DisableReadOnly() = tracked, because we may update it (refresh expired token).
    /// Used instead of ActivePickupQrSpecification to handle all 5 cases:
    ///   1. No record exists       → create new
    ///   2. Valid (active) record  → return as-is
    ///   3. Expired, not used      → refresh (update same row)
    ///   4. IsUsed = true          → 400 error (never regenerate)
    /// </summary>
    public class AnyPickupQrByRequestSpecification : BaseSpecification<QrToken>
    {
        public AnyPickupQrByRequestSpecification(int bloodRequestId)
        {
            Criteria = q =>
                q.BloodRequestId == bloodRequestId &&
                q.Type == QrTokenType.Pickup;

            DisableReadOnly();  // tracked — may update Token/ExpiryDate in place
        }
    }


    /// <summary>
    /// Finds ANY Pickup QrToken linked to a Donation — regardless of IsUsed or expiry.
    /// DisableReadOnly() = tracked, because we may refresh an expired token in place.
    /// Mirrors AnyPickupQrByRequestSpecification but for general donation withdrawals.
    /// </summary>
    public class AnyPickupQrByDonationSpecification : BaseSpecification<QrToken>
    {
        public AnyPickupQrByDonationSpecification(int donationId)
        {
            Criteria = q =>
                q.DonationId == donationId &&
                q.Type == QrTokenType.Pickup;

            DisableReadOnly();  // tracked — may update Token/ExpiryDate in place
        }
    }
}
