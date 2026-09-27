namespace Trader.Infrastructure.Scheduler
{
    public class ClockSyncOptions
    {
        public const string SectionName = "ServerClock:AutoSync";

        /// <summary>زمان‌های sync به وقت ایران (HH:mm:ss)</summary>
        public List<string> SyncTimes { get; set; } = new()
        {
            "08:44:56",
            "11:44:56",
        };

        /// <summary>تعداد نمونه در هر sync</summary>
        public int SamplesPerSync { get; set; } = 5;

        /// <summary>منطقه زمانی</summary>
        public string TimeZone { get; set; } = "Asia/Tehran";
    }
}