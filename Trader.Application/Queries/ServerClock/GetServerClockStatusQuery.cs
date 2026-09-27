 
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.ServerClock
{
    public record GetServerClockStatusQuery()
        : IRequest<Result<ServerClockInfoView>>;

    public class GetServerClockStatusQueryHandler
        : IRequestHandler<GetServerClockStatusQuery, Result<ServerClockInfoView>>
    {
        private readonly IServerClockQueryService _clockQueryService;
        private readonly ILogger<GetServerClockStatusQueryHandler> _logger;

        public GetServerClockStatusQueryHandler(
            IServerClockQueryService clockQueryService,
            ILogger<GetServerClockStatusQueryHandler> logger)
        {
            _clockQueryService = clockQueryService;
            _logger = logger;
        }

        public async Task<Result<ServerClockInfoView>> Handle(
            GetServerClockStatusQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Server Clock Status");

                var info = await _clockQueryService.GetStatusAsync();
                return Result<ServerClockInfoView>.Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Server Clock Status");
                return Result<ServerClockInfoView>.Fail(ex.Message);
            }
        }
    }
}