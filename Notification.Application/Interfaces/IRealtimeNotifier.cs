using Notification.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Notification.Application.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task SendToUserAsync(string userId, NotificationMessage message, CancellationToken ct = default);
        Task BroadcastAsync(NotificationMessage message, CancellationToken ct = default);
    }

    public interface IWebPushSender
    {
        Task SendAsync(string userId, NotificationMessage message, CancellationToken ct = default);
    }
}
