using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Application.DTOs.HR;
using Core.Domain.Specifications;
using Microsoft.Extensions.Logging;
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
    public class OrganizationService : IOrganizationService
    {
        private readonly ISpecificationRepository<OrganizationUnit, Guid> _unitSpecRepository;
        private readonly IRepository<OrganizationManagementDbContext, OrganizationUnit, Guid> _unitRepository;
        private readonly ISpecificationRepository<Position, Guid> _positionSpecRepository;
        private readonly IRepository<OrganizationManagementDbContext,Position, Guid> _positionRepository;
        private readonly ISpecificationRepository<Assignment, Guid> _assignmentSpecRepository;
        private readonly IRepository<OrganizationManagementDbContext,Assignment, Guid> _assignmentRepository;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(
            ISpecificationRepository<OrganizationUnit, Guid> unitSpecRepository,
            IRepository<OrganizationManagementDbContext, OrganizationUnit, Guid> unitRepository,
            ISpecificationRepository<Position, Guid> positionSpecRepository,
        IRepository<OrganizationManagementDbContext, Position, Guid> positionRepository,
        ISpecificationRepository<Assignment, Guid> assignmentSpecRepository,
        IRepository<OrganizationManagementDbContext, Assignment, Guid> assignmentRepository,
        ILogger<OrganizationService> logger)
        {
            _unitSpecRepository = unitSpecRepository;
            _unitRepository = unitRepository;
            _positionRepository = positionRepository;
            _positionSpecRepository = positionSpecRepository;
            _assignmentRepository = assignmentRepository;
            _assignmentSpecRepository = assignmentSpecRepository;
            _logger = logger;
        }

        public async Task<PositionDto?> GetUserPositionAsync(Guid userId)
        {
            try
            {
                _logger.LogDebug("Getting position for user {UserId}", userId);

                // استفاده از Specification شیک
                var assignmentSpec = new ActiveAssignmentsByPersonSpec(userId);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                var assignment = assignments.FirstOrDefault();
                if (assignment == null)
                {
                    _logger.LogWarning("No active assignment found for user {UserId}", userId);
                    return null;
                }

                var position = assignment.Position;
                if (position == null)
                {
                    _logger.LogError("Position not found for assignment {AssignmentId}", assignment.Id);
                    return null;
                }

                return new PositionDto
                {
                    Id = position.Id,
                    Title = position.Title,
                    Code = position.Code,
                    OrganizationUnitId = position.FkOrganizationUnitId,
                    OrganizationUnitName = position.OrganizationUnit?.Name ?? string.Empty,
                    OrganizationUnitCode = position.OrganizationUnit?.Code ?? string.Empty,
                    IsManagerial = position.IsManagerial,
                    ReportsToPositionId = position.ReportsToPositionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetUserUnitsAsync(Guid userId)
        {
            try
            {
                _logger.LogDebug("Getting units for user {UserId}", userId);

                var position = await GetUserPositionAsync(userId);
                if (position == null)
                    return new List<Guid>();

                var unitId = position.OrganizationUnitId;
                var units = new List<Guid> { unitId };

                if (position.IsManagerial)
                {
                    var childUnits = await GetChildUnitsAsync(unitId);
                    units.AddRange(childUnits);
                }

                return units.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user units for {UserId}", userId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetAvailableUnitsAsync(Guid userId)
        {
            try
            {
                _logger.LogDebug("Getting available units for user {UserId}", userId);

                var userUnits = await GetUserUnitsAsync(userId);
                var position = await GetUserPositionAsync(userId);

                if (position?.IsManagerial == true)
                {
                    var allUnits = await _unitRepository.GetAllAsync();
                    return allUnits.Select(u => u.Id).ToList();
                }

                return userUnits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available units for {UserId}", userId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetSubtreeUnitsAsync(Guid unitId, int depth)
        {
            try
            {
                if (depth <= 0)
                    return new List<Guid> { unitId };

                var allUnits = await _unitRepository.GetAllAsync();
                var unitDict = allUnits.ToDictionary(u => u.Id, u => u);

                if (!unitDict.ContainsKey(unitId))
                    return new List<Guid>();

                var result = new List<Guid> { unitId };
                var queue = new Queue<(Guid unitId, int currentDepth)>();
                queue.Enqueue((unitId, 0));

                while (queue.Count > 0)
                {
                    var (currentUnitId, currentDepth) = queue.Dequeue();

                    if (currentDepth >= depth)
                        continue;

                    // استفاده از Specification برای گرفتن فرزندان
                    var childSpec = new OrganizationUnitsByParentSpec(currentUnitId);
                    var children = await _unitSpecRepository.ListBySpecAsync(childSpec);

                    foreach (var child in children)
                    {
                        if (!result.Contains(child.Id))
                        {
                            result.Add(child.Id);
                            queue.Enqueue((child.Id, currentDepth + 1));
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subtree units for unit {UnitId}", unitId);
                throw;
            }
        }

        public async Task<string?> GetUnitPathAsync(Guid unitId)
        {
            try
            {
                var unit = await _unitRepository.GetByIdAsync(unitId);
                return unit?.Path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting path for unit {UnitId}", unitId);
                throw;
            }
        }

        public async Task<bool> IsUnitInHierarchyAsync(Guid parentUnitId, Guid childUnitId)
        {
            try
            {
                if (parentUnitId == childUnitId)
                    return true;

                var childUnit = await _unitRepository.GetByIdAsync(childUnitId);
                if (childUnit == null)
                    return false;

                var current = childUnit;
                while (current.ParentId.HasValue)
                {
                    if (current.ParentId.Value == parentUnitId)
                        return true;

                    current = await _unitRepository.GetByIdAsync(current.ParentId.Value);
                    if (current == null)
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking hierarchy for units {ParentUnitId} and {ChildUnitId}",
                    parentUnitId, childUnitId);
                throw;
            }
        }

        public async Task<OrganizationUnitDto?> GetUnitAsync(Guid unitId)
        {
            try
            {
                var unit = await _unitRepository.GetByIdAsync(unitId);
                if (unit == null)
                    return null;

                var parent = unit.ParentId.HasValue
                    ? await _unitRepository.GetByIdAsync(unit.ParentId.Value)
                    : null;

                var manager = await GetUnitManagerAsync(unitId);
                var childCount = await GetUnitChildCountAsync(unitId);
                var userCount = await GetUnitUserCountAsync(unitId);

                return new OrganizationUnitDto
                {
                    Id = unit.Id,
                    Name = unit.Name,
                    Code = unit.Code,
                    Description = unit.Description,
                    ParentId = unit.ParentId,
                    ParentName = parent?.Name,
                    Path = unit.Path,
                    Level = CalculatePathLevel(unit.Path),
                    IsActive = true,
                    CreatedAt = unit.CreatedAt,
                    ManagerId = manager?.Id,
                    ManagerName = manager?.Title,
                    UserCount = userCount,
                    ChildCount = childCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unit {UnitId}", unitId);
                throw;
            }
        }

        public async Task<PositionDto?> GetUnitManagerAsync(Guid unitId)
        {
            try
            {
                // استفاده از Specification
                var managerSpec = new PositionsByUnitManagerialSpec(unitId);

                var managers = await _positionSpecRepository.ListBySpecAsync(managerSpec);
                var manager = managers.FirstOrDefault();

                if (manager == null)
                    return null;

                return new PositionDto
                {
                    Id = manager.Id,
                    Title = manager.Title,
                    Code = manager.Code,
                    OrganizationUnitId = unitId,
                    OrganizationUnitName = manager.OrganizationUnit?.Name ?? string.Empty,
                    OrganizationUnitCode = manager.OrganizationUnit?.Code ?? string.Empty,
                    IsManagerial = true,
                    ReportsToPositionId = manager.ReportsToPositionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manager for unit {UnitId}", unitId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetUnitUsersAsync(Guid unitId)
        {
            try
            {
                // 1. گرفتن Positionهای واحد با Specification
                var positionSpec = new PositionsByUnitSpec(unitId, includeOrganizationUnit: false);
                var positions = await _positionSpecRepository.ListBySpecAsync(positionSpec);
                var positionIds = positions.Select(p => p.Id).ToList();

                if (!positionIds.Any())
                    return new List<Guid>();

                // 2. گرفتن Assignmentهای فعال با Specification
                var assignmentSpec = new ActiveAssignmentsByPositionsSpec(positionIds);
                var assignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

                return assignments.Select(a => a.FkPersonId).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for unit {UnitId}", unitId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetSiblingUnitsAsync(Guid unitId)
        {
            try
            {
                var unit = await _unitRepository.GetByIdAsync(unitId);
                if (unit?.ParentId == null)
                    return new List<Guid>();

                var siblingSpec = new OrganizationUnitsByParentSpec(unit.ParentId);
                var siblings = await _unitSpecRepository.ListBySpecAsync(siblingSpec);

                // حذف خود واحد از لیست
                return siblings.Where(u => u.Id != unitId).Select(u => u.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting siblings for unit {UnitId}", unitId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Guid>> GetChildUnitsAsync(Guid unitId)
        {
            try
            {
                var childSpec = new OrganizationUnitsByParentSpec(unitId);
                var children = await _unitSpecRepository.ListBySpecAsync(childSpec);
                return children.Select(u => u.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting children for unit {UnitId}", unitId);
                throw;
            }
        }

        // متدهای کمکی
        private async Task<int> GetUnitChildCountAsync(Guid unitId)
        {
            var childSpec = new OrganizationUnitsByParentSpec(unitId);
            return await _unitSpecRepository.CountBySpecAsync(childSpec);
        }

        private async Task<int> GetUnitUserCountAsync(Guid unitId)
        {
            var users = await GetUnitUsersAsync(unitId);
            return users.Count;
        }

        private int CalculatePathLevel(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            return path.Count(c => c == '/');
        }
    }
}
