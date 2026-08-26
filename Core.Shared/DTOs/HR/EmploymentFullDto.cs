using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.HR
{
    public class EmploymentFullDto
    {
        public Guid Id { get; set; }
        public Guid? ProfileId { get; set; }
        public Guid? PartyProfileId { get; set; }

        public string NationalCode { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmploymentCode { get; set; } = null!;

        public DateOnly EmploymentEffectiveFrom { get; set; }

        public DateOnly? EmploymentEffectiveTo { get; set; }

        public Guid PartyId { get; set; }

        public string? EmploymentStatusName { get; set; }

        public string? EmploymentTypeName { get; set; }

        public List<PostInfoDto> posts { get; set; } = null;

        public List<LocationInfoDto> empLocations { get; set; } = null;
        public List<LocationInfoDto> postLocations { get; set; } = null;
    }
}
