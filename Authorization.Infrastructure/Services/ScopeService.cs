using Authorization.Application.Interfaces.Processor;
using Authorization.Application.Interfaces.Service;
using Authorization.Domain.Entities;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Shared.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class ScopeService : IScopeInternalService
    {
        private readonly IRepository<AuthorizationDbContext, Scope, Guid> _scopeRepository;
        private readonly ISpecificationRepository<Scope, Guid> _scopeSpecRepository;
        private readonly IScopeProcessor _scopeProcessor;

        private readonly IUnitOfWork<AuthorizationDbContext> _unitOfWork;
        private readonly ILogger<PermissionService> _logger;
        private readonly ICachePublicService _cache;
        public ScopeService(IScopeProcessor scopeProcessor,
            IRepository<AuthorizationDbContext, Scope, Guid> scopeRepository,
        ISpecificationRepository<Scope, Guid> scopeSpecRepository,
         IUnitOfWork<AuthorizationDbContext> unitOfWork,
        ILogger<PermissionService> logger,
        ICachePublicService cache
            )
        {
            _scopeProcessor = scopeProcessor;
            _scopeRepository = scopeRepository;
            _scopeSpecRepository = scopeSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cache = cache;
        }
        public async Task UpdateScopeOfPermission(Guid permissionId, List<ScopeType>? newScopes)
        {
           var existScopeList = await GetPermissionScopesList( permissionId );
            //bool isEqual = await _scopeProcessor.compareTwoScopeList(existScopeList.Select(s => s.scope).ToList(), newScopes);
            if (!existScopeList.Equals(newScopes))
            {
                await _scopeRepository.RemoveRangeAsync(existScopeList);
                await AddScopesToPermission(permissionId, newScopes);

            }

        }


        public async Task AddScopesToPermission(Guid permissionId, List<ScopeType>? scopes)
        {
            List<Scope> newList = scopes.Select(s=>new Scope
            {
                scope = s,
                PermissionId = permissionId
            }).ToList();
            await _scopeRepository.AddRangeAsync(newList);
        }


        private async Task<List<Scope>> GetPermissionScopesList(Guid permissionId)
        {
          var list = await _scopeSpecRepository.ListBySpecAsync(new GetScopesByPermissionIdSpec(permissionId));
            return list.ToList();
        }
    }
}
