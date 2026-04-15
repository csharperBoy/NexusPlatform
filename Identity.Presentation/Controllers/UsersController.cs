using Core.Application.Context;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
using Identity.Application.Commands.User;
using Identity.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Presentation.Controllers
{
    [ApiController]
    [Route("api/identity/[controller]")]
    public class UsersController : BaseController
    {
        private readonly UserDataContext _currentUser;

        public UsersController(UserDataContext currentUser)
        {
            _currentUser = currentUser;
        }
        [HttpGet("GetUsers")]
        [AuthorizeResource("identity.user", "View")]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery? request = null)
        {

            //var query = new GetUsersQuery(request.UserName , request.NickName, request.phoneNumber);
            var result = await Mediator.Send(request);
            return HandleResult(result);
        }
        [HttpGet("{id:guid}")]
        [AuthorizeResource("identity.user", "View")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }


        /// <summary>
        /// ✏️ به‌روزرسانی کاربر
        /// </summary>
        [HttpPut("{id:guid}")]
        [AuthorizeResource("identity.user", "Edit")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            // اطمینان از تطابق ID در route با command
            var updatedCommand = command with { Id = id };
            var result = await Mediator.Send(updatedCommand);
            return HandleResult(result);
        }
        [HttpPost("Create")]
        [AuthorizeResource("identity.user", "Create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        /// <summary>
        /// 🗑️ حذف کاربر
        /// </summary>
        [HttpDelete("{id:guid}")]
        [AuthorizeResource("identity.user", "Delete")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var command = new DeleteUserCommand(id);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            if (_currentUser.UserId == Guid.Empty)
                return Unauthorized();

            return Ok(new
            {
                _currentUser.UserId,
                _currentUser.UserName
            });
        }
    }
}
