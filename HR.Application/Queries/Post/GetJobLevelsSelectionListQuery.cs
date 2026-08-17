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

    public record GetJobLevelsSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetJobLevelsSelectionListQueryHandler : IRequestHandler<GetJobLevelsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IPostInternalService _service;
        public GetJobLevelsSelectionListQueryHandler(IPostInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetJobLevelsSelectionListQuery request, CancellationToken ct)
        {
            IEnumerable<JobLevel> list = await _service.GetJobLevelListAsync();
            var result = list.ToList().Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Title}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
