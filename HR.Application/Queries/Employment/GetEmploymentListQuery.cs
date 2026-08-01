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

namespace HR.Application.Queries.Employment
{
    public record GetEmploymentListQuery(string? employmentCode = null)
     : IRequest<Result<IReadOnlyList<EmployementInfoView>>>;

    public class GetEmploymentListQueryHandler
        : IRequestHandler<GetEmploymentListQuery, Result<IReadOnlyList<EmployementInfoView>>>
    {
        private readonly IEmployeeInternalService _employmentInternalService;
        private readonly ILogger<GetEmploymentListQueryHandler> _logger;

        public GetEmploymentListQueryHandler(
            IEmployeeInternalService employmentInternalService,
        ILogger<GetEmploymentListQueryHandler> logger)
        {
            _employmentInternalService = employmentInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<EmployementInfoView>>> Handle(
            GetEmploymentListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Employment List:");

                var posts = await _employmentInternalService.GetEmploymentListAsync();
                return Result<IReadOnlyList<EmployementInfoView>>.Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Employment List");
                return Result<IReadOnlyList<EmployementInfoView>>.Fail(ex.Message);
            }
        }
    }
}
