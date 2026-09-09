using Core.Domain.Common;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Services
{
    public class OrganizationUnitService:IOrganizationUnitInternalService
    {
        private readonly IHRUnitOfWork<HRDbContext> _unitOfWork;
        private readonly ILogger<OrganizationUnitService> _logger;

        public OrganizationUnitService(IHRUnitOfWork<HRDbContext> unitOfWork, ILogger<OrganizationUnitService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Guid> CreateAsync(string code, string name,Guid? parentId, string createBy)
        {
            OrganizationUnit newModel = new OrganizationUnit(name, code,parentId );
            await _unitOfWork.OrganizationUnitRepository.AddAsync(newModel);
            return newModel.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            OrganizationUnit? model = await _unitOfWork.OrganizationUnitRepository.GetByIdAsync(id);
            if (model == null)
            {
                _logger.LogWarning("Organization Unit not found for deletion.");
                throw new Exception($"Organization Unit with ID {id} not found.");
            }
            await _unitOfWork.OrganizationUnitRepository.DeleteAsync(model);
        }

        public async Task SaveAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Guid id, Optional<string?> name, Optional<string?> code, Optional<Guid?> parentId)
        {
            bool hasChange = false;
            OrganizationUnit? model = await _unitOfWork.OrganizationUnitRepository.GetByIdAsync(id);
            if (model == null)
            {
                _logger.LogWarning("Organization Unit not found for update.");
                throw new Exception($"Organization Unit with ID {id} not found.");
            }

            hasChange = model.ApplyChange(name, code,parentId);

            await _unitOfWork.OrganizationUnitRepository.UpdateAsync(model);
            return true;
        }
    }
}
