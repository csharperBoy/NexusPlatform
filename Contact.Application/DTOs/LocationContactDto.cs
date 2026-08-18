using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.DTOs
{
    
    public class LocationContactDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<string>? orgMobile { get; set; }
        public List<string>? orgPhone { get; set; }

        //public List<EntityContactDto<HrContactType>> Contacts { get; set; } = null;
    }
}
