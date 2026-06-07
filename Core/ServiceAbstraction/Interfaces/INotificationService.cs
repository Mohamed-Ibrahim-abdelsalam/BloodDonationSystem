using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);

         /// <summary>
        /// Creates and persists a notification for a specific user.
        /// Used internally by other services — never called from controllers directly.
        
           Task SendAsync(
                string receiverUserId,
                string title,
                string message,
                int?   referenceId   = null,
                string? referenceType = null);
    }
    
}
