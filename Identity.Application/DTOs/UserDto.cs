using Core.Domain.ValueObjects;
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
        //public Guid FkPersonId { get;  set; }

        public string? FirstName { get;  set; }
        public string? LastName { get;  set; }
        public string? Email { get;  set; }
    }
}
