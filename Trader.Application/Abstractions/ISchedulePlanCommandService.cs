using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface ISchedulePlanCommandService
    {
        Task<Guid> CreateSchedulePlanAsync(
            string name,
            string date,
            bool enabled,
            string autoLoginAt,
            string autoRefreshAt,
            List<ScheduledOrderItemDto> orders);

        Task<Guid> UpdateSchedulePlanAsync(
            Guid id,
            string name,
            string date,
            bool enabled,
            string autoLoginAt,
            string autoRefreshAt,
            List<ScheduledOrderItemDto> orders);

        Task<bool> DeleteSchedulePlanAsync(Guid id);

        Task<bool> EnableAsync(Guid id);

        Task<bool> DisableAsync(Guid id);

        Task SaveAsync();
    }
}