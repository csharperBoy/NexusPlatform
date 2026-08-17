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

namespace HR.Application.Queries.CostCenter
{
   
    public record GetCostCentersSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetCostCentersSelectionListQueryHandler : IRequestHandler<GetCostCentersSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IPostInternalService _service;
        public GetCostCentersSelectionListQueryHandler(IPostInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetCostCentersSelectionListQuery request, CancellationToken ct)
        {
            IEnumerable<HR.Domain.Entities.CostCenter> list = await _service.GetCostCenterListAsync();
            var result = list.ToList().Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Name}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
