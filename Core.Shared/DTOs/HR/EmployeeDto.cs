using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.HR
{
    public class EmployeeDto
    {
        #region party
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        #endregion
        #region Person
        public string NationalCode { get; set; }
        public string FirstlName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? FatherName { get; set; }
        public Gender? Gender { get; set; }
        #endregion
        #region employee
        public string EmployeeCode { get; set; }
        public string EmploymentTypeTitle { get; set; }
        public Guid EmploymentStatusTitle { get; set; }
        
        public string locations { get; set; }
        #endregion
        #region post assign
        public string PostCode { get;  set; }
        public Guid OrganizationUnit { get;  set; }
        public Guid JobTitle { get;  set; }
        public Guid? JobLevel { get;  set; }
        public Guid? Grade { get;  set; }
        public Guid? CostCenter { get;  set; }

        public string? PostPhone { get; set; }
        public string? PostEmail { get; set; }
        public string? PostMobile { get; set; }
        #endregion
    }
}
