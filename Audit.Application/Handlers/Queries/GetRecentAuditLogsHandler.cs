using Audit.Application.Interfaces;
using Audit.Application.Query;
using Audit.Domain.Entities;
using Audit.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.Handlers.Queries
{
    public class GetRecentAuditLogsHandler
    //: IRequestHandler<GetRecentAuditLogsQuery, IEnumerable<AuditLog>>
        
       : IRequestHandler<GetRecentAuditLogsQuery, Result<IReadOnlyList<AuditLog>>>

    {
        //private readonly ISpecificationRepository< AuditLog, Guid> _repo;
        private readonly IAuditQueryService _service;

        public GetRecentAuditLogsHandler(
            //ISpecificationRepository<AuditLog, Guid> repo
            IAuditQueryService service
            )
        {
            _service = service;
        }


       async Task<Result<IReadOnlyList<AuditLog>>> IRequestHandler<GetRecentAuditLogsQuery, Result<IReadOnlyList<AuditLog>>>.Handle(GetRecentAuditLogsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetRecentLogsAsync(1,request.Count);
        }
    }

}
