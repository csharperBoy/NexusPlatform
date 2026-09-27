 
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Commands.ServerClock
{
    public record SyncServerClockCommand(int Samples = 5)
        : IRequest<Result<ServerClockInfoView>>;

    public class SyncServerClockCommandHandler
        : IRequestHandler<SyncServerClockCommand, Result<ServerClockInfoView>>
    {
        private readonly IServerClockCommandService _clockService;
        private readonly ILogger<SyncServerClockCommandHandler> _logger;

        public SyncServerClockCommandHandler(
            IServerClockCommandService clockService,
            ILogger<SyncServerClockCommandHandler> logger)
        {
            _clockService = clockService;
            _logger = logger;
        }

        public async Task<Result<ServerClockInfoView>> Handle(
            SyncServerClockCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Syncing server clock with {Samples} samples",
                    request.Samples);

                var info = await _clockService.SyncAsync(request.Samples);

                _logger.LogInformation(
                    "Clock synced: diff={Diff}ms offset={Offset}ms",
                    info.Diff, info.Offset);

                return Result<ServerClockInfoView>.Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync server clock");
                return Result<ServerClockInfoView>.Fail(ex.Message);
            }
        }
    }
}