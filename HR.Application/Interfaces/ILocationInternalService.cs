using Core.Application.Abstractions.HR;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using HR.Application.DTOs;
using HR.Domain.Entities;
using HR.Domain.Specifications;
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

         List<string>? _orgPhone = null,
         List<string>? _orgEmail = null,
         List<string>? _orgMobile = null
             );
        Task<Guid> UpdateLocationAsync(Guid id, string? title, List<string>? officePhone, List<string>? orgEmail, List<string>? orgMobile);
      
        Task<IReadOnlyList<LocationInfoDto>> GetLocationListAsync();
        Task DeleteAsync(Guid id);

    }
}
