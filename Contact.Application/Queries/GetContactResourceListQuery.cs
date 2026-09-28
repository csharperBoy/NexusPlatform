using Contact.Application.DTOs;
using Contact.Application.Interfaces;
using Core.Shared.DTOs;
using Core.Shared.DTOs.HR;
using Core.Shared.Results;
using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Queries
{
    public record GetContactResourceListQuery() : IRequest<Result<IList<ContactResourceInfoDto>>>;

    public class GetContactResourceListQueryHandler : IRequestHandler<GetContactResourceListQuery, Result<IList<ContactResourceInfoDto>>>
    {
        private readonly IContactResourceQueryService _service;
        public GetContactResourceListQueryHandler(IContactResourceQueryService service)
        {
            _service = service;
        }

        public async Task<Result<IList<ContactResourceInfoDto>>> Handle(GetContactResourceListQuery request, CancellationToken ct)
        {
            return await _service.GetContactResourceListAsync(request);
        }
    }
   
}
