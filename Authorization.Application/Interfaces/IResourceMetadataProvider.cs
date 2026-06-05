using Authorization.Application.DTOs.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IResourceMetadataProvider
    {
        IReadOnlyList<ResourceMetadataDto> Resources { get; }
        ResourceMetadataDto? GetMetadata(string resourceKey);
    }
}
