using Auth.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IEnumerable<string> roles);
    }
}
