using Microsoft.AspNetCore.SignalR;
using Notification.Application.Interfaces;
using Notification.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Infrastructure.RealTime
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRRealtimeNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(string userId, NotificationMessage message, CancellationToken ct = default)
        {
            await _hubContext.Clients.User(userId).SendAsync("notifications/new", message, ct);
        }

        public async Task BroadcastAsync(NotificationMessage message, CancellationToken ct = default)
        {
            await _hubContext.Clients.All.SendAsync("notifications/new", message, ct);
        }
    }
}
