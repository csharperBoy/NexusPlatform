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

         string? _orgPhone = null,
         string? _orgEmail = null,
         string? _orgMobile = null
             );
        Task<Guid> UpdateLocationAsync(Guid id, string? title, string? officePhone, string? orgEmail, string? orgMobile);
      
        Task<IReadOnlyList<LocationInfoDto>> GetLocationListAsync();

    }
}
