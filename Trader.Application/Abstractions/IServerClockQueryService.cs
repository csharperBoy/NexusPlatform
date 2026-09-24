using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IServerClockQueryService
    {
        Task<ServerClockInfoView> GetStatusAsync();
    }
}