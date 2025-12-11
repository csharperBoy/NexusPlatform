using Core.Application.Abstractions.Security;
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
    public class UsersController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        public UsersController(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        [HttpGet("me")]
        //[Authorize]
        public IActionResult Me()
        {
            if (!_currentUser.IsAuthenticated)
                return Unauthorized();

            return Ok(new
            {
                _currentUser.UserId,
                _currentUser.UserName,
                Roles = _currentUser.Roles
            });
        }
    }
}
