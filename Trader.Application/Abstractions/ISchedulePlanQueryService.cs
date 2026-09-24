using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface ISchedulePlanQueryService
    {
        Task<IReadOnlyList<SchedulePlanInfoView>> GetSchedulePlanListAsync();

        Task<SchedulePlanInfoView?> GetSchedulePlanByIdAsync(Guid id);
    }
}