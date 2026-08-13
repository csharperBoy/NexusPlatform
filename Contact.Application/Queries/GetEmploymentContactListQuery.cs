using Contact.Application.DTOs;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Queries
{
    
    public record GetEmploymentContactListQuery(string? employmentCode = null)
   : IRequest<Result<IReadOnlyList<EmploymentContactDto>>>;

    public class GetEmploymentContactListQueryHandler
        : IRequestHandler<GetEmploymentContactListQuery, Result<IReadOnlyList<EmploymentContactDto>>>
    {
        private readonly IEmploymentInternalService _employmentInternalService;
        private readonly ILogger<GetEmploymentContactListQueryHandler> _logger;

        public GetEmploymentContactListQueryHandler(
            IEmploymentInternalService employmentInternalService,
        ILogger<GetEmploymentContactListQueryHandler> logger)
        {
            _employmentInternalService = employmentInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<EmploymentContactDto>>> Handle(
            GetEmploymentContactListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting EmploymentContact List:");

                var employments = await _employmentInternalService.GetEmploymentListAsync();
                IReadOnlyList<EmploymentContactDto> result = employments
                    .Where(e => string.IsNullOrEmpty(request.employmentCode) || e.EmploymentCode == request.employmentCode)
                    .Select(e => new EmploymentContactDto
                    {
                        Id = e.Id,
                        NationalCode = e.NationalCode,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        EmploymentCode = e.EmploymentCode,
                        PartyMobile = e.PartyMobile,
                        PartyAddress = e.PartyAddress,
                        PartyPhone = e.PartyPhone,
                        PartyEmail = e.PartyEmail,
                        PostContactPhone = e.PostContactPhone,
                        PostContactMobile = e.PostContactMobile,
                        PostContactEmail = e.PostContactEmail,
                        PostContactFax = e.PostContactFax,
                        EmploymentContactPhone = e.EmploymentContactPhone,
                        EmploymentContactMobile = e.EmploymentContactMobile,
                        EmploymentContactEmail = e.EmploymentContactEmail,
                        EmploymentContactFax = e.EmploymentContactFax
                    })
                    .ToList();
                return Result<IReadOnlyList<EmploymentContactDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get EmploymentContact List");
                return Result<IReadOnlyList<EmploymentContactDto>>.Fail(ex.Message);
            }
        }
    }
}
