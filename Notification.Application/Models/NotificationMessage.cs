using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Notification.Application.Models
{
    public enum NotificationChannel
    {
        Email,
        WebPush,
        InApp
    }

    public class NotificationMessage
    {
        public string UserId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public string? IconUrl { get; set; }
        public string? LinkUrl { get; set; }
        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
        public IDictionary<string, string>? Metadata { get; set; }
    }
}
