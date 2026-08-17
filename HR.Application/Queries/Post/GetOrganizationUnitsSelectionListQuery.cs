using Core.Shared.DTOs;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Queries.Post
{
    public record GetOrganizationUnitsSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetOrganizationUnitsSelectionListQueryHandler : IRequestHandler<GetOrganizationUnitsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IPostInternalService _service;
        public GetOrganizationUnitsSelectionListQueryHandler(IPostInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetOrganizationUnitsSelectionListQuery request, CancellationToken ct)
        {
            IEnumerable<OrganizationUnit> list = await _service.GetOrganizationUnitListAsync();
            var result = list.ToList().Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Name}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
