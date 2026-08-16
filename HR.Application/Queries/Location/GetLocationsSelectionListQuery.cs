using Core.Shared.DTOs;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Queries.Location
{
    
    public record GetLocationsSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetLocationsSelectionListQueryHandler : IRequestHandler<GetLocationsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly ILocationInternalService _service;
        public GetLocationsSelectionListQueryHandler(ILocationInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetLocationsSelectionListQuery request, CancellationToken ct)
        {
            var resources = await _service.GetLocationListAsync();
            var result = resources.Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Title}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
