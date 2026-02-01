using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.HR
{
    public interface IPositionPublicService
    {
        Task<List<Guid>> GetUserPositionsId(Guid userId);
    }
}
