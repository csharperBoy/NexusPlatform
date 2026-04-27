using Core.Application.Abstractions.Identity.PublicService;
using Core.Shared.DTOs;
using Identity.Application.Commands.User;
using Identity.Application.DTOs;
using Identity.Application.Queries.User;
using Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Interfaces
{
    public interface IUserInternalService : IUserPublicService
    {
        Task<Guid> CreateUserAsync(CreateUserCommand request);
        Task DeleteUserAsync(Guid id);

        Task<UserDto?> GetById(Guid id);
        Task<IReadOnlyList<UserDto>> GetUsers(string? UserName = null, List<Guid>? rolesId = null, string? NickName = null, string? phoneNumber = null);
        Task UpdateUserAsync(UpdateUserCommand request);
    }
}
