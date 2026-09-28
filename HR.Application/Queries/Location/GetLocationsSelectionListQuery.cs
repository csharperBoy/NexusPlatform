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
    
    public record GetLocationsSelectionListQuery() : IRequest<Result<IList<TreeSelectionListDto>>>;

    public class GetLocationsSelectionListQueryHandler : IRequestHandler<GetLocationsSelectionListQuery, Result<IList<TreeSelectionListDto>>>
    {
        private readonly ILocationInternalService _service;
        public GetLocationsSelectionListQueryHandler(ILocationInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<TreeSelectionListDto>>> Handle(GetLocationsSelectionListQuery request, CancellationToken ct)
        {
            var resources = await _service.GetLocationListAsync();
            var result = resources.Select(x => new TreeSelectionListDto(x.Id.ToString(), $"{x.Title}" , x.ParentId.ToString()));
            return Result<IList<TreeSelectionListDto>>.Ok(result.ToList());
        }
    }
}
