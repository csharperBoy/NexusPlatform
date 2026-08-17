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

    public record GetJobTitlesSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetJobTitlesSelectionListQueryHandler : IRequestHandler<GetJobTitlesSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IPostInternalService _service;
        public GetJobTitlesSelectionListQueryHandler(IPostInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetJobTitlesSelectionListQuery request, CancellationToken ct)
        {
            IEnumerable<JobTitle> list = await _service.GetJobTitleListAsync();
            var result = list.ToList().Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Name}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
