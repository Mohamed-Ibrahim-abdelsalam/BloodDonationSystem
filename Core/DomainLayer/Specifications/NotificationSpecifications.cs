using BloodDonationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Specifications
{
    /// <summary>
    /// GET /api/notifications — all notifications for a user, newest first
    /// </summary>
    public class NotificationsByUserSpecification : BaseSpecification<Notification>
    {
        public NotificationsByUserSpecification(string userId)
        {
            Criteria = n => n.UserId == userId;
            ApplyOrderByDesc(n => n.CreatedAt);
        }
    }

    /// <summary>
    /// GET single notification by id — for ownership check
    /// </summary>
    public class NotificationByIdSpecification : BaseSpecification<Notification>
    {
        public NotificationByIdSpecification(int id)
        {
            Criteria = n => n.Id == id;
        }
    }
}
