using Audit.Application.Query;
using Audit.Domain.Entities;
using Audit.Domain.Specifications;
using Core.Application.Abstractions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.Handlers
{
    public class GetRecentAuditLogsHandler
    : IRequestHandler<GetRecentAuditLogsQuery, IEnumerable<AuditLog>>
    {
        private readonly ISpecificationRepository<AuditLog, Guid> _repo;

        public GetRecentAuditLogsHandler(ISpecificationRepository<AuditLog, Guid> repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<AuditLog>> Handle(GetRecentAuditLogsQuery request, CancellationToken ct)
        {
            var spec = new RecentAuditLogsSpec(request.Count);
            return await _repo.ListBySpecAsync(spec);
        }
    }

}
