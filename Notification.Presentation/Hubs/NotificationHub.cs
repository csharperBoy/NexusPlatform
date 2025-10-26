using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Presentation.Hubs
{
    [Authorize] // برای ارسال به کاربرهای احراز شده
    public class NotificationHub : Hub
    {
        // اختیاری: متدهای مدیریت اتصال/گروه‌ها
        public override Task OnConnectedAsync()
        {
            // می‌تونی بر اساس نقش/پرمیشن گروه اضافه کنی
            return base.OnConnectedAsync();
        }
    }
}