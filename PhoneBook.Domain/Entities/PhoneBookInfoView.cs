using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBook.Domain.Entities
{
    public class PhoneBookInfoView
    {

        public string NationalCode { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmployeeCode { get; set; } = null!;




        public string? JobLevelTitle { get; set; }

        public string? JobTitleName { get; set; }

        public string? OrganizationUnitsName { get; set; }

        public string? LocationTitle { get; set; }




        public string? PartyMobile { get; set; }

        public string? PartyPhone { get; set; }

        public string? PartyEmail { get; set; }

        public string? PartyAddress { get; set; }
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
