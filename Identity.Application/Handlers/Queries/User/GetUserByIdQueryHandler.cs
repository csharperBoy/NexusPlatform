using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.User;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Handlers.Queries.User
{
    public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserInternalService _UserService;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;

        public GetUserByIdQueryHandler(
            IUserInternalService UserService,
            ILogger<GetUserByIdQueryHandler> logger)
        {
            _UserService = UserService;
            _logger = logger;
        }

        public async Task<Result<UserDto>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting User by ID: {UserId}", request.Id);

                ApplicationUser? User = await _UserService.GetById(request.Id);

                if (User == null)
                {
                    return Result<UserDto>.Fail($"User with ID {request.Id} not found");
                }

                UserDto result = new UserDto()
                {
                    FirstName = User.FullName?.FirstName,
                    LastName =User.FullName?.LastName,
                    Email = User.Email,
                    Id = User.Id,
                    phoneNumber = User.PhoneNumber,
                    UserName = User.UserName
                };
                return Result<UserDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get User by ID: {UserId}", request.Id);
                return Result<UserDto>.Fail(ex.Message);
            }
        }
    }

}
