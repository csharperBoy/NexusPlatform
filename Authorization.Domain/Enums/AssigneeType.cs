using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum AssigneeType : byte
    {
        Role = 1,
        Position = 2,
        Person = 3  // اولویت بالاتر
    }
}
