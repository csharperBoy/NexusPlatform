using Core.Shared.DTOs;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HR.Application.Interfaces;
namespace HR.Domain.Specifications
{
    public record GetEmploymentsSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetEmploymentsSelectionListQueryHandler : IRequestHandler<GetEmploymentsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IEmploymentInternalService _service;
        public GetEmploymentsSelectionListQueryHandler(IEmploymentInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetEmploymentsSelectionListQuery request, CancellationToken ct)
        {
            var resources = await _service.GetEmploymentListAsync();
            var result = resources.Select(x => new SelectionListDto(x.Id.ToString(), $"{x.FirstName} {x.LastName}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
