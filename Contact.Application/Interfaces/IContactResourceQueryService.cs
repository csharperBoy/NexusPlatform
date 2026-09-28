using Contact.Application.DTOs;
using Contact.Application.Queries;
using Core.Shared.DTOs;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Interfaces
{
    public interface IContactResourceQueryService
    {
        Task<Result<IList<TreeSelectionListDto>>> GetContactResourcesSelectionListAsync(GetContactResourcesSelectionListQuery request);
        Task<Result<IList<ContactResourceInfoDto>>> GetContactResourceListAsync(GetContactResourceListQuery request);
    }
}
