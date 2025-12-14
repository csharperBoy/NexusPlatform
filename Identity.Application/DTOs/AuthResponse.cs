using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTOs
{
    public record AuthResponse(
     string AccessToken,
     string RefreshToken,
     Guid userId,
     DateTime ExpiresAt,
     string UserName
 );

}