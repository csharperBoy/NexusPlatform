using Contact.Application.Interfaces;
using Core.Shared.DTOs;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Queries
{
    
    public record GetContactResourcesSelectionListQuery() : IRequest<Result<IList<TreeSelectionListDto>>>;

    public class GetContactResourcesSelectionListQueryHandler : IRequestHandler<GetContactResourcesSelectionListQuery, Result<IList<TreeSelectionListDto>>>
    {
        private readonly IContactResourceQueryService _service;
        public GetContactResourcesSelectionListQueryHandler(IContactResourceQueryService service)
        {
            _service = service;
        }

        public async Task<Result<IList<TreeSelectionListDto>>> Handle(GetContactResourcesSelectionListQuery request, CancellationToken ct)
        {
            return await _service.GetContactResourcesSelectionListAsync(request);
        }
    }
}
