using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Microsoft.Extensions.Logging;
using OrganizationManagement.Application.Interfaces;
using OrganizationManagement.Domain.Entities;
using OrganizationManagement.Domain.Specifications;
using OrganizationManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Infrastructure.Services
{
    public class PositionService : IPositionInternalService, IPositionPublicService
    {
        private readonly ISpecificationRepository<Position, Guid> _positionSpecRepository;
        private readonly IRepository<OrganizationManagementDbContext, Position, Guid> _positionRepository;
        private readonly ISpecificationRepository<Assignment, Guid> _assignmentSpecRepository;
        private readonly ILogger<PositionService> _logger;

        public PositionService(
            ISpecificationRepository<Position, Guid> positionSpecRepository,
        IRepository<OrganizationManagementDbContext, Position, Guid> positionRepository, ISpecificationRepository<Assignment, Guid> assignmentSpecRepository,
        ILogger<PositionService> logger)
        {
            _positionRepository = positionRepository;
            _positionSpecRepository = positionSpecRepository;
            _logger = logger;
            _assignmentSpecRepository = assignmentSpecRepository;
        }

        public async Task<List<Guid>?> GetUserPositionsId(Guid userId)
        {
            try
            {
                var positions = await GetUserPositionAsync(userId);
                return positions.Select(p=>p.Id).ToList();
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public async Task<List<Position>?> GetUserPositionAsync(Guid userId)
        {
            try
            {
                _logger.LogDebug("Getting position for user {UserId}", userId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByPersonSpec(userId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                var assignment = assignments.ToList();
                if (assignment == null)
                {
                    _logger.LogWarning("No active assignment found for user {UserId}", userId);
                    return null;
                }

                var position = assignment.Select(a=>a.Position);
                if (position == null)
                {
                    _logger.LogError("Position not found for assignment ");
                    return null;
                }

                return position.ToList();
          
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Guid>?> GetUserOrganizeId(Guid userId)
        {
            try
            {
                var positions = await GetUserPositionAsync(userId);
                return positions.Select(p => p.FkOrganizationUnitId).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
