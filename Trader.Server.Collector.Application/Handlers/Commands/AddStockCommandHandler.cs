using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Application.Commands;
using Trader.Server.Collector.Application.Interface;

namespace Trader.Server.Collector.Application.Handlers.Commands
{
    public class AddStockCommandHandler : IRequestHandler<AddStockCommand, Result<Guid>>
    {
        private readonly IStockService _stockService;
        private readonly ILogger<AddStockCommandHandler> _logger;

        public AddStockCommandHandler(
            IStockService stockService,
            ILogger<AddStockCommandHandler> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(AddStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Add Stock for {Isin}:{Title} ",
                    request.Isin, request.Title);

                var stockId = await _stockService.AddStock(request);

                _logger.LogInformation(
                    "Add Stock successfully: {Isin}:{Title} ",
                    request.Isin, request.Title);

                return Result<Guid>.Ok(Guid.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Add Stock for  {Isin}:{Title} ",
                    request.Title, request.Isin);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
