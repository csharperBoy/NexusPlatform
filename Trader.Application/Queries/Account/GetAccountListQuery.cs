using Core.Application.Results;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.Account
{
    public record GetAccountListQuery(string? Search = null)
        : IRequest<Result<IReadOnlyList<AccountInfoView>>>;

    public class GetAccountListQueryHandler
        : IRequestHandler<GetAccountListQuery, Result<IReadOnlyList<AccountInfoView>>>
    {
        private readonly IAccountQueryService _accountQueryService;
        private readonly ILogger<GetAccountListQueryHandler> _logger;

        public GetAccountListQueryHandler(
            IAccountQueryService accountQueryService,
            ILogger<GetAccountListQueryHandler> logger)
        {
            _accountQueryService = accountQueryService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<AccountInfoView>>> Handle(
            GetAccountListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Trader Account List");

                var accounts = await _accountQueryService.GetAccountListAsync();
                return Result<IReadOnlyList<AccountInfoView>>.Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Trader Account List");
                return Result<IReadOnlyList<AccountInfoView>>.Fail(ex.Message);
            }
        }
    }
}