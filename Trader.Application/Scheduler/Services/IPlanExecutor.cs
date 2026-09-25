namespace Trader.Application.Scheduler.Services
{
    public interface IPlanExecutor
    {
        Task ExecuteAsync(Guid planId, CancellationToken ct = default);
    }
}