using Audit.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Domain.Specifications
{
    public class RecentAuditLogsSpec : BaseSpecification<AuditLog>
    {
        public RecentAuditLogsSpec(int page = 1, int pageSize = 10)
        {
            ApplyOrderByDescending(l => l.Timestamp);

            var skip = (page - 1) * pageSize;
            ApplyPaging(skip, pageSize);
        }
    }
}
