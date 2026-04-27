using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Resource;
using Authorization.Domain.Entities;
using Core.Shared.DTOs;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Resource
{
    public class GetResourcesSelectionListQueryHandler : IRequestHandler<GetResourcesSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly IResourceInternalService _resourceService;
        public GetResourcesSelectionListQueryHandler(IResourceInternalService resourceService)
            => _resourceService = resourceService;

        public async Task<Result<IList<SelectionListDto>>> Handle(GetResourcesSelectionListQuery request, CancellationToken ct)
        {
            var resources = await _resourceService.GetResources();
            var result = resources.Select(x => new SelectionListDto(x.Id.ToString(), $"{x.Name}"));
            return Result<IList<SelectionListDto>>.Ok(result.ToList());
        }
    }
    

}
