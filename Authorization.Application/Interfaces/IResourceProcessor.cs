using Authorization.Application.DTOs.Resource;
using Authorization.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IResourceProcessor
    {
        /// <summary>
        /// ساخت درخت کامل منابع
        /// </summary>
        IReadOnlyList<ResourceTreeDto> BuildTree(IEnumerable<Resource> resources, Guid? parentId = null);

    }
}
