using Core.Shared.Results;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Interface;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Queries
{
    public record GetEmploymentQuery( int Page = 1, int PageSize = 10)
        : IRequest<Result<IReadOnlyList<PdsIdeaInformationViw>>>;

    public class GetEmploymentQueryHandler
      : IRequestHandler<GetEmploymentQuery, Result<IReadOnlyList<PdsIdeaInformationViw>>>
    {
        private readonly ISyncService _service;

        public GetEmploymentQueryHandler(ISyncService service)
        {
            _service = service;
        }

        public async Task<Result<IReadOnlyList<PdsIdeaInformationViw>>> Handle(GetEmploymentQuery request, CancellationToken ct)
        {
             //await _service.SyncEmployements();
            var lst = await _service.GetEmployment();
            return Result<IReadOnlyList<PdsIdeaInformationViw>>.Ok(lst);
        }
    }
}
