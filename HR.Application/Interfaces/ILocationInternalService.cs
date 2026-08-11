using Core.Application.Abstractions.HR;
using Core.Domain.ValueObjects;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface ILocationInternalService : ILocationPublicService
    {
        //Task AssignLocationsToLocation(Guid LocationId, List<Guid> locationsId);
        Task<Guid> CreateLocationAsync(
           string _title,

         string? _orgPhone = null,
         Email? _orgEmail = null,
         string? _orgMobile = null
             );
        Task<IReadOnlyList<LocationInfoView>> GetLocationListAsync();
        Task<Guid> UpdateLocationAsync(Guid id, string? phone, string? address, string? email, string? mobile, string? firstlName, string? lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string? LocationCode, Guid? LocationTypeId, Guid? LocationStatusId, DateOnly? startDate, DateOnly? endDate, List<Guid>? locationsId, string? officePhone, string? orgEmail, string? orgMobile);
    }
}
