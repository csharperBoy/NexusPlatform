using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.DTOs
{
    public class EmploymentContactDto
    {
        public Guid Id { get; set; }
        public string NationalCode { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmploymentCode { get; set; } = null!;

       

        public List<string>? PartyMobile { get; set; }

        public List<string>? PartyAddress { get; set; }

        public List<string>? PartyPhone { get; set; }

        public List<string>? PartyEmail { get; set; }


        public List<string>? EmploymentContactPhone { get; set; }
        public List<string>? EmploymentContactMobile { get; set; }

        //public List<EntityContactDto<HrContactType>> Contacts { get; set; } = null;
        //public List<EntityContactDto<PartyContactType>> partyContacts { get; set; } = null;

    }
}
