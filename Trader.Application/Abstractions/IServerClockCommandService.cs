using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface IServerClockCommandService
    {
        Task<ServerClockInfoView> SyncAsync(int samples);
    }
}