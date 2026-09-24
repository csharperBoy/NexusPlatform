using Core.Application.Dtos;   // ← فرض: SelectionListDto اینجاست (مطابق HR)
using Core.Application.Results;
using Core.Shared.DTOs;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Queries.Account
{
    public record GetAccountsSelectionListQuery()
        : IRequest<Result<IList<SelectionListDto>>>;

    public class GetAccountsSelectionListQueryHandler
        : IRequestHandler<GetAccountsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IAccountQueryService _accountQueryService;
        private readonly ILogger<GetAccountsSelectionListQueryHandler> _logger;

        public GetAccountsSelectionListQueryHandler(
            IAccountQueryService accountQueryService,
            ILogger<GetAccountsSelectionListQueryHandler> logger)
        {
            _accountQueryService = accountQueryService;
            _logger = logger;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(
            GetAccountsSelectionListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var accounts = await _accountQueryService.GetAccountListAsync();

                var result = accounts
                    .Select(x => new SelectionListDto(x.Id.ToString(), x.Name))
                    .ToList();

                return Result<IList<SelectionListDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Trader Account Selection List");
                return Result<IList<SelectionListDto>>.Fail(ex.Message);
            }
        }
    }
}