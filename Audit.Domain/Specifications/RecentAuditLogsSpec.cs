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
        public RecentAuditLogsSpec(int count)
        {
            ApplyOrderByDescending(l => l.Timestamp);
            ApplyPaging(0, count);
        }
    }
}
