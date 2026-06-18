using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Enums
{
    public enum ContactType : byte
    {
        Phone = 0,
        Mobile = 1,
        Fax = 2,
        Address=3,
        Email=4,
        TelegramId=5,
        Instagram = 6,
        EitaaId = 7,
    }
}
