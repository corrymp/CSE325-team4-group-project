using System;

namespace Plan2Gather.Models
{
    public class AppNotification
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
