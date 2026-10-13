using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface ISchedulePlanCommandService
    {
        /// <summary>
        /// ساخت برنامه زمانبندی جدید
        /// </summary>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <param name="enabled"></param>
        /// <param name="autoLoginAt"></param>
        /// <param name="autoRefreshAt"></param>
        /// <param name="fireLeadMs"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        Task<Guid> CreateSchedulePlanAsync(
     string name,
     string date,
     bool enabled,
     string autoLoginAt,
     string autoRefreshAt,
     int fireLeadMs,
     List<ScheduledOrderItemDto> orders);

        /// <summary>
        /// بروزرسانی برنامه زمانبندی
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <param name="enabled"></param>
        /// <param name="autoLoginAt"></param>
        /// <param name="autoRefreshAt"></param>
        /// <param name="fireLeadMs"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        Task<Guid> UpdateSchedulePlanAsync(
            Guid id,
            string name,
            string date,
            bool enabled,
            string autoLoginAt,
            string autoRefreshAt,
            int fireLeadMs,
            List<ScheduledOrderItemDto> orders);

        /// <summary>
        /// حذف برنامه زمانبندی
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteSchedulePlanAsync(Guid id);

        /// <summary>
        /// فعالسازی برنامه زمانبندی و ایجاد متناظر آن در ماژول Scheduler جهت اجرا
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task EnableAsync(Guid id);
        /// <summary>
        /// غیرفعالسازی برنامه زمانبندی و حذف متناظر آن در ماژول Scheduler
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DisableAsync(Guid id);
        /// <summary>
        /// ذخیره تغییرات در دیتابیس
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();
    }
}