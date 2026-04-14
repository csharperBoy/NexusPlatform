using Core.Application.Abstractions.Identity.PublicService;
using Identity.Application.DTOs;
using Identity.Application.Queries.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Interfaces
{
    public interface IUserInternalService : IUserPublicService
    {
        Task<List<UserDto>> getUsers(GetUsersQuery request);
    }
}
