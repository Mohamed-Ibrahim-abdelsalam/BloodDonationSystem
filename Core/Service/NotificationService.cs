using AutoMapper;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ── GET /api/notifications ────────────────────────────────────────────
        public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(string userId)
        {
            var spec = new NotificationsByUserSpecification(userId);
            var notifications = await _uow.Notifications.GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        // ── PUT /api/notifications/{id}/read ──────────────────────────────────
        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var spec = new NotificationByIdSpecification(notificationId);
            var notification = await _uow.Notifications.GetEntityWithSpecAsync(spec);

            // 404
            if (notification is null)
                throw new KeyNotFoundException(
                    $"Notification with id {notificationId} was not found.");

            // 403 — not the owner
            if (notification.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to update this notification.");

            notification.IsRead = true;
            _uow.Notifications.Update(notification);
            await _uow.SaveChangesAsync();
        }
    }
}
