using Audit.Domain.Entities;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.Query
{
    public record GetRecentAuditLogsQuery(int Count) : IRequest<Result<IReadOnlyList<AuditLog>>>;

}
