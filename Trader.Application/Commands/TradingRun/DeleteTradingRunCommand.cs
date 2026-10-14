using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.TradingRun;

public record DeleteTradingRunCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteTradingRunCommandHandler
    : IRequestHandler<DeleteTradingRunCommand, Result<bool>>
{
    private readonly ITradingRunCommandService _service;
    private readonly ILogger<DeleteTradingRunCommandHandler> _logger;

    public DeleteTradingRunCommandHandler(
        ITradingRunCommandService service,
        ILogger<DeleteTradingRunCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteTradingRunCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(request.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete TradingRun {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}