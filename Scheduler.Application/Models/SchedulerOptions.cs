using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Application.Models
{

    public class SchedulerOptions
    {
        public const string SectionName = "Scheduler";

        /// <summary>صف پیش‌فرض برای اجرای jobها</summary>
        public string DefaultQueue { get; set; } = "scheduler";

        /// <summary>
        /// چقدر قبل از fireAt واقعی، Hangfire trigger بشه.
        /// داخل job با PreciseDelay به fireAt واقعی می‌رسیم.
        /// </summary>
        public TimeSpan DefaultPreFireBuffer { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>Hangfire polling interval برای صف delayed</summary>
        public int SchedulePollIntervalSeconds { get; set; } = 5;

        /// <summary>تعداد worker برای صف scheduler</summary>
        public int WorkerCount { get; set; } = 0; // 0 = auto
    }
}
