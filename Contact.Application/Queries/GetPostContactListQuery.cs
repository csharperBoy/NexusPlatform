using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Contact;
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

        private readonly IContactPublicService _contactService;
        public GetPostContactListQueryHandler(
            IPostInternalService orgChartInternalService, IContactPublicService contactService,
        ILogger<GetPostContactListQueryHandler> logger)
        {
            _orgChartInternalService = orgChartInternalService;
            _contactService = contactService;
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
                var postProfIds = posts.Select(p => p.ProfileId).ToList();

                var contactList = await _contactService.GetContactsByProfilesIdsAsync(postProfIds);

                IReadOnlyList<PostContactDto> result = posts.Select(post => new PostContactDto
                {
                    Id = post.Id,
                    FkParentId = post.FkParentId,
                    CostCenterName = post.CostCenterName,
                    GradeTitle = post.GradeTitle,
                    OrganizationUnitsName = post.OrganizationUnitsName,
                    JobLevelTitle = post.JobLevelTitle,
                    JobTitleName = post.JobTitleName,
                    OfficePhone = contactList.Where(a =>  a.ContactType == ContactTypeEnum.OfficePhone && a.IsCurrent)?.Select(a => a.Value).ToList(),
                    OrgMobile = contactList.Where(a =>  a.ContactType == ContactTypeEnum.OrganizationMobile && a.IsCurrent)?.Select(a => a.Value).ToList(),
                    OrgEmail = contactList.Where(a => a.ContactType == ContactTypeEnum.Email && a.IsCurrent)?.Select(a => a.Value).ToList(),
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
