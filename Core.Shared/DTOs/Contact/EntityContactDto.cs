using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Contact
{
    public class EntityContactDto<TContactType> where TContactType : Enum
    {
        public TContactType ContactType { get;  set; }
        public string Value { get;  set; }
        public Guid EntityId { get;  set; }

        public bool IsCurrent { get;  set; }

    }
}
