using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.HR;
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
        private readonly IPostInternalService _orgChartInternalService;
        private readonly ILogger<GetPostContactListQueryHandler> _logger;

        private readonly IHrContactPublicService _hrContactService;
        public GetPostContactListQueryHandler(
            IPostInternalService orgChartInternalService, IHrContactPublicService hrContactService,
        ILogger<GetPostContactListQueryHandler> logger)
        {
            _orgChartInternalService = orgChartInternalService;
            _hrContactService = hrContactService;
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
                var postIds = posts.Select(p => p.Id).ToList();

                var hrContactList = await _hrContactService.GetLocationContactsByLocationIdsAsync(postIds);

                IReadOnlyList<PostContactDto> result = posts.Select(post => new PostContactDto
                {
                    Id = post.Id,
                    FkParentId = post.FkParentId,
                    CostCenterName = post.CostCenterName,
                    GradeTitle = post.GradeTitle,
                    OrganizationUnitsName = post.OrganizationUnitsName,
                    JobLevelTitle = post.JobLevelTitle,
                    JobTitleName = post.JobTitleName,
                    OfficePhone = hrContactList.Where(a => a.EntityId == post.Id && a.ContactType == HrContactType.OfficePhone)?.Select(a => a.Value).ToList(),
                    OrgMobile = hrContactList.Where(a => a.EntityId == post.Id && a.ContactType == HrContactType.OrgMobile)?.Select(a => a.Value).ToList(),
                    OrgEmail = hrContactList.Where(a => a.EntityId == post.Id && a.ContactType == HrContactType.OrgEmail)?.Select(a => a.Value).ToList(),
                    EmploymentCode = post.EmploymentCode,
                    FirstName = post.FirstName,
                    LastName = post.LastName,
                    Gender = post.Gender,
                    AssignmentsAssigneeType = post.AssigneeType
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
