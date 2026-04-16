using Core.Domain.ValueObjects;
using Core.Shared.DTOs.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTOs
{
    public class UserDto
    {

        public Guid Id { get;  set; }
        public string? UserName { get;  set; }
        public string? phoneNumber { get;  set; }
        public string? NickName { get; set; }
        public string? Email { get;  set; }
        public PersonDto? person { get; set; } = null;
        //public ICollection<RoleDto>? roles { get; set; } = null;
        public ICollection<string>? roles { get; set; } = null;
    }
}
