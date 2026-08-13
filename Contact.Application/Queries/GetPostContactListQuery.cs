using Contact.Application.DTOs;
using Core.Shared.DTOs.Authorization;
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
    public record GetPostContactListQuery(Guid? rootId = null)
        : IRequest<Result<IReadOnlyList<PostContactDto>>>;

    public class GetPostContactListQueryHandler
        : IRequestHandler<GetPostContactListQuery, Result<IReadOnlyList<PostContactDto>>>
    {
        private readonly IOrgChartInternalService _orgChartInternalService;
        private readonly ILogger<GetPostContactListQueryHandler> _logger;

        public GetPostContactListQueryHandler(
            IOrgChartInternalService orgChartInternalService,
        ILogger<GetPostContactListQueryHandler> logger)
        {
            _orgChartInternalService = orgChartInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<PostContactDto>>> Handle(
            GetPostContactListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting PostContact List:");

                var posts = await _orgChartInternalService.GetPostListAsync();
                IReadOnlyList<PostContactDto> result = posts.Select(post => new PostContactDto
                {
                    Id = post.Id,
                    FkParentId = post.FkParentId,
                    JobLevelTitle = post.JobLevelTitle,
                    JobTitleName = post.JobTitleName,
                    OfficePhone = post.OfficePhone,
                    OrgMobile = post.OrgMobile,
                    OrgEmail = post.OrgEmail,
                    EmploymentCode = post.EmploymentCode,
                    FirstName = post.FirstName,
                    LastName = post.LastName,
                    Gender = post.Gender,
                    AssignmentsAssigneeType = post.AssignmentsAssigneeType
                }).ToList();
                return Result<IReadOnlyList<PostContactDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PostContact List");
                return Result<IReadOnlyList<PostContactDto>>.Fail(ex.Message);
            }
        }
    }
}
