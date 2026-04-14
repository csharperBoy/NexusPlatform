using Core.Application.Context;
using Core.Presentation.Controllers;
using Core.Presentation.Filters;
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

            var query = new GetUsersQuery(request.UserName , request.FullName, request.phoneNumber);
            var result = await Mediator.Send(query);
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
