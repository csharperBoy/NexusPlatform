using Authorization.Application.DTOs.Resource;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Resource
{
   
    public record GetResourceMetaDataQuery(Guid id)
      : IRequest<Result<List<ResourceMetadataDto>>>;
}
