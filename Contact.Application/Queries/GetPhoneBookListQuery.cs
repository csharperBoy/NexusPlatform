using Contact.Application.DTOs;
using Contact.Application.Interfaces;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Queries
{
    
    public record GetPhoneBookListQuery(Guid? organUnitId = null)
     : IRequest<Result<IReadOnlyList<PhoneBookEmploymentDto>>>;

    public class GetPhoneBookListQueryHandler
        : IRequestHandler<GetPhoneBookListQuery, Result<IReadOnlyList<PhoneBookEmploymentDto>>>
    {
        private readonly IPhoneBookInternalService _phoneBookInternalService;
        private readonly ILogger<GetPhoneBookListQueryHandler> _logger;

        public GetPhoneBookListQueryHandler(
            IPhoneBookInternalService phoneBookInternalService,
        ILogger<GetPhoneBookListQueryHandler> logger)
        {
            _phoneBookInternalService = phoneBookInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<PhoneBookEmploymentDto>>> Handle(
            GetPhoneBookListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting PhoneBook Info List:");

                var list = await _phoneBookInternalService.GetPhoneBookListAsync(request.organUnitId);
                return Result<IReadOnlyList<PhoneBookEmploymentDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PhoneBook Info List");
                return Result<IReadOnlyList<PhoneBookEmploymentDto>>.Fail(ex.Message);
            }
        }
    }
}
