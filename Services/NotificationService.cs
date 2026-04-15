using System;
using System.Collections.Generic;
using System.Linq;
using Plan2Gather.Models;

namespace Plan2Gather.Services
{
    public class NotificationService
    {
        private readonly List<AppNotification> _notifications = new();

        public event Action? OnChange;

        public IReadOnlyList<AppNotification> Notifications => _notifications;

        public void Add(AppNotification notification)
        {
            _notifications.Insert(0, notification);
            OnChange?.Invoke();
        }

        public void Add(string message)
        {
            Add(new AppNotification
            {
                Id = Guid.NewGuid(),
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }

        public void MarkRead(Guid id)
        {
            var n = _notifications.FirstOrDefault(x => x.Id == id);
            if (n != null && !n.IsRead)
            {
                n.IsRead = true;
                OnChange?.Invoke();
            }
        }

        public int UnreadCount => _notifications.Count(x => !x.IsRead);
    }
}
