using Core.Shared.Results;
using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Queries.Employment
{
    public record GetEmploymentListQuery(string? employmentCode = null)
     : IRequest<Result<IReadOnlyList<EmploymentInfoDto>>>;

    public class GetEmploymentListQueryHandler
        : IRequestHandler<GetEmploymentListQuery, Result<IReadOnlyList<EmploymentInfoDto>>>
    {
        private readonly IEmploymentInternalService _employmentInternalService;
        private readonly ILogger<GetEmploymentListQueryHandler> _logger;

        public GetEmploymentListQueryHandler(
            IEmploymentInternalService employmentInternalService,
        ILogger<GetEmploymentListQueryHandler> logger)
        {
            _employmentInternalService = employmentInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<EmploymentInfoDto>>> Handle(
            GetEmploymentListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Employment List:");

                var employments = await _employmentInternalService.GetEmploymentListAsync();
                return Result<IReadOnlyList<EmploymentInfoDto>>.Ok(employments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Employment List");
                return Result<IReadOnlyList<EmploymentInfoDto>>.Fail(ex.Message);
            }
        }
    }
}
