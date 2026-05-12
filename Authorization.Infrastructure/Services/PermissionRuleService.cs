using Authorization.Application.Interfaces.Service;
using Authorization.Domain.Entities;
using Authorization.Domain.Events;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Data;
using Azure.Core;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Context;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class PermissionRuleService : IPermissionRuleInternalService
    {
        private readonly IRepository<AuthorizationDbContext, PermissionRule, Guid> _permissionRuleRepository;
        private readonly ISpecificationRepository<PermissionRule, Guid> _permissionRuleSpecRepository;
        private readonly IUnitOfWork<AuthorizationDbContext> _unitOfWork;
        private readonly ILogger<PermissionRuleService> _logger;
        private readonly ICachePublicService _cache;
        private readonly UserDataContext _currentUserContext;
        private readonly string baseCacheKey = "authorization:permissionRule";

        public PermissionRuleService(
            IRepository<AuthorizationDbContext, PermissionRule, Guid> permissionRuleRepository,
            ISpecificationRepository<PermissionRule, Guid> permissionRuleSpecRepository,
            IUnitOfWork<AuthorizationDbContext> unitOfWork,
            ILogger<PermissionRuleService> logger,
            UserDataContext currentUserContext,
            ICachePublicService cache)
        {
            _permissionRuleRepository = permissionRuleRepository;
            _permissionRuleSpecRepository = permissionRuleSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserContext = currentUserContext;
            _cache = cache;
        }

        public async Task<Guid> CreateRuleAsync(Guid permissionId, string fieldName, ComparisonOperator comparisonOperator, string? value, LogicalOperator logicalOperator, int groupOrder)
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission Rule create ");


                // ایجاد Permission جدید
                var permissionRule = new PermissionRule(
                     permissionId,
                     fieldName,
                     comparisonOperator,    
                     value,
                     logicalOperator,
                     groupOrder
                     
                );

                // ذخیره در Repository
                await _permissionRuleRepository.AddAsync(permissionRule);

                // انتشار ایونت
                //permissionRule.AddDomainEvent(new PermissionChangedEvent(AssigneeId, ResourceId));
                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();
                /*
                await _scopeService.AddScopesToPermission(permission.Id, scopes);

                await _unitOfWork.SaveChangesAsync();
                // پاک کردن کش‌های مرتبط
                await InvalidatePermissionCachesAsync(AssigneeId, ResourceId);
                */
                await InvalidatePermissionRuleCachesAsync();
                _logger.LogInformation(
                    "Permission rule create successfully");

                return permissionRule.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create permission rule ");
                throw;
            }
        }
        private async Task InvalidatePermissionRuleCachesAsync()
        {
            try
            {
                await _cache.RemoveByPatternAsync($"{baseCacheKey}:*");

                _logger.LogDebug("Invalidated permission Rule caches ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating permission Rule caches  ");
            }
        }
        public async Task DeletePermissionRuleAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Starting permissionRule Delete : {id}", id);

                await _permissionRuleRepository.DeleteAsync(id);

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidatePermissionRuleCachesAsync();

                _logger.LogInformation("Permission Rule Delete successfully: {id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed Permission Rule Delete : {id}", id);
                throw;
            }
        }

        public async Task<PermissionRuleDto?> GetById(Guid id)
        {
            try
            {
                var permission = await _permissionRuleRepository.GetByIdAsync(id, p => p.JoinDetail);

                if (permission == null)
                {
                    _logger.LogWarning("PermissionRule not found: {id}", id);
                    return null;
                }

                PermissionRuleDto dto = MapToDto(permission);
                _logger.LogDebug("Retrieved permissionRule: {id}", id);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission: {id}", id);
                throw;
            }
        }

        private PermissionRuleDto MapToDto(PermissionRule model)
        {              

            return new PermissionRuleDto()
            {
                Id = model.Id,  
                PermissionId = model.PermissionId,
                FieldName = model.FieldName,
                GroupOrder = model.GroupOrder,
                Operator = model.Operator,
                LogicalOperator = model.LogicalOperator,
                Value = model.Value,
                JoinEntity = model.JoinDetail?.JoinEntity,
                JoinForeignKey = model.JoinDetail?.JoinForeignKey,
                JoinLocalKey = model.JoinDetail?.JoinLocalKey,
            };
        
        }

        public async Task<IReadOnlyList<PermissionRuleDto>> GetPermissionRules(Guid? permissionId)
        {
            var cacheKey = $"{baseCacheKey}:full";
            var cached = await _cache.GetAsync<IReadOnlyList<PermissionRuleDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for full resource tree");
                return cached;
            }
            var spec = new GetPermissionRulesSpec(permissionId);
            var permissions = await _permissionRuleSpecRepository.ListBySpecAsync(spec);


            var result = permissions.Select(MapToDto).ToList();


            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            return result;
        }

        public async Task UpdatePermissionRuleAsync(Guid id, Guid? permissionId, string? fieldName, ComparisonOperator? comparisonOperator, string? value, LogicalOperator? logicalOperator, int? groupOrder)
        {
            try
            {
                _logger.LogInformation(
                    "Starting permission rule update");

                var permissionRule = await _permissionRuleRepository.GetByIdAsync(id);
                if (permissionRule == null)
                {
                    throw new ArgumentException($"Permission with ID {id} not found");
                }
                permissionRule.ApplyChange(permissionId,  fieldName, comparisonOperator,  value,  logicalOperator,  groupOrder);
                // انتشار ایونت
                //permissionRule.AddDomainEvent(new PermissionChangedEvent((Guid)AssigneeId, (Guid)ResourceId));
                await _permissionRuleRepository.UpdateAsync(permissionRule);
                await _unitOfWork.SaveChangesAsync();
                await InvalidatePermissionRuleCachesAsync();

                _logger.LogInformation("Permission rule update successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update permission rule");
                throw;
            }
        }
    }
}
