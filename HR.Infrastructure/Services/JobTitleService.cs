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
    public class JobTitleService: IJobTitleInternalService
    {
        private readonly IHRUnitOfWork<HRDbContext> _unitOfWork;
        private readonly ILogger<JobTitleService> _logger;

        public JobTitleService(IHRUnitOfWork<HRDbContext> unitOfWork, ILogger<JobTitleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Guid> CreateAsync(string code, string name, string createBy)
        {
            JobTitle newModel = new JobTitle(code, name);
            await _unitOfWork.JobTitleRepository.AddAsync(newModel);
            return newModel.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            JobTitle? model = await _unitOfWork.JobTitleRepository.GetByIdAsync(id);
            if (model == null)
            {
                _logger.LogWarning("Job title not found for deletion.");
                throw new Exception($"Job title with ID {id} not found.");
            }
            await _unitOfWork.JobTitleRepository.DeleteAsync(model);
        }

        public async Task SaveAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Guid id, Optional<string?> name, Optional<string?> code, Optional<bool> isActive)
        {
            bool hasChange = false;
            JobTitle? model = await _unitOfWork.JobTitleRepository.GetByIdAsync(id);
            if (model == null)
            {
                _logger.LogWarning("Job title not found for update.");
                throw new Exception($"Job title with ID {id} not found.");
            }

            hasChange =  model.ApplyChange(name, code, isActive);

            await _unitOfWork.JobTitleRepository.UpdateAsync(model);
            return true;
        }
    }
}
