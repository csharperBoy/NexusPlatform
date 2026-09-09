using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
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
    public class JobLevelService : IJobLevelInternalService
    {
        private readonly IHRUnitOfWork<HRDbContext> _unitOfWork;
        private readonly ILogger<JobLevelService> _logger;

        public JobLevelService(IHRUnitOfWork<HRDbContext> unitOfWork, ILogger<JobLevelService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Guid> CreateAsync(string code, string title, string createBy)
        {
            JobLevel newModel = new JobLevel(code, title);
            await _unitOfWork.JobLevelRepository.AddAsync(newModel);
            return newModel.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            JobLevel? model = await _unitOfWork.JobLevelRepository.GetByIdAsync(id);
            if (model == null)
            {
                _logger.LogWarning("Job level not found for deletion.");
                throw new Exception($"Job level with ID {id} not found.");
            }
            await _unitOfWork.JobLevelRepository.DeleteAsync(model);
        }

        public async Task SaveAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Guid id, Optional<string?> title, Optional<string?> code)
        {
            bool hasChange = false;
            JobLevel? model = await _unitOfWork.JobLevelRepository.GetByIdAsync(id);
            if (model == null)
            {
                _logger.LogWarning("Job level not found for update.");
                throw new Exception($"Job level with ID {id} not found.");
            }

            model.ApplyChange(title, code);

            await _unitOfWork.JobLevelRepository.UpdateAsync(model);
            return true;
        }
    }
}
