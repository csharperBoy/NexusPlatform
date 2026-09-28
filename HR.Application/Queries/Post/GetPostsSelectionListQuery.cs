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
    public record GetPostsSelectionListQuery() : IRequest<Result<IList<TreeSelectionListDto>>>;

    public class GetPostsSelectionListQueryHandler : IRequestHandler<GetPostsSelectionListQuery, Result<IList<TreeSelectionListDto>>>
    {
        private readonly IPostInternalService _service;
        public GetPostsSelectionListQueryHandler(IPostInternalService service)
        {
            _service = service;
        }

        public async Task<Result<IList<TreeSelectionListDto>>> Handle(GetPostsSelectionListQuery request, CancellationToken ct)
        {
            var resources = await _service.GetPostListAsync();
            var result = resources.Select(x => new TreeSelectionListDto(x.Id.ToString(), $"{x.FkJobTitleId}" , x.ParentId.ToString()));
            return Result<IList<TreeSelectionListDto>>.Ok(result.ToList());
        }
    }
}
