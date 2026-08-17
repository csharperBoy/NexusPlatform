using Core.Shared.DTOs;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Queries.Post
{
    public record GetPostsSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

    public class GetPostsSelectionListQueryHandler : IRequestHandler<GetPostsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IPostInternalService _service;
        public GetPostsSelectionListQueryHandler(IPostInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(GetPostsSelectionListQuery request, CancellationToken ct)
        {
            var resources = await _service.GetPostListAsync();
            var result = resources.Select(x => new SelectionListDto(x.Id.ToString(), $"{x.FkJobTitleId}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
}
