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

       

        public string? PartyMobile { get; set; }

        public string? PartyAddress { get; set; }

        public string? PartyPhone { get; set; }

        public string? PartyEmail { get; set; }

      

        public string? PostContactPhone { get; set; }

        public string? PostContactMobile { get; set; }

        public string? PostContactEmail { get; set; }

        public string? PostContactFax { get; set; }

        public string? EmploymentContactPhone { get; set; }
        public string? EmploymentContactMobile { get; set; }
        public string? EmploymentContactEmail { get; set; }
        public string? EmploymentContactFax { get; set; }
      
    }
}
