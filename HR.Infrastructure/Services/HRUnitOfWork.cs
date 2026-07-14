using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
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
    public class HRUnitOfWork : EfUnitOfWork<HRDbContext>, IHRUnitOfWork<HRDbContext>
    {
        public HRUnitOfWork(
            IRepository<HRDbContext, Assignment, Guid> assignmentRepository,
            IRepository<HRDbContext, Employment, Guid> employmentRepository,
            IRepository<HRDbContext, EmploymentLocation, Guid> employmentLocationRepository,
            IRepository<HRDbContext, EmploymentStatus, Guid> employmentStatusRepository,
            IRepository<HRDbContext, EmploymentType, Guid> employmentTypeRepository,
            IRepository<HRDbContext, Post, Guid> postRepository,
            IRepository<HRDbContext, CostCenter, Guid> costCenterRepository,
            IRepository<HRDbContext, Grade, Guid> gradeRepository,
            IRepository<HRDbContext, JobLevel, Guid> jobLevelRepository,
            IRepository<HRDbContext, JobTitle, Guid> jobTitleRepository,
            IRepository<HRDbContext, Location, Guid> locationRepository,
            IRepository<HRDbContext, OrganizationUnit, Guid> organizationUnitRepository,
            IRepository<HRDbContext, PostContact, Guid> postContactRepository,
            HRDbContext dbContext,
            IOutboxService<HRDbContext> outboxService,
            ILogger<EfUnitOfWork<HRDbContext>> logger) : base(dbContext, outboxService, logger)
        {
            AssignmentRepository = assignmentRepository;
            EmploymentRepository = employmentRepository;
            EmploymentLocationRepository = employmentLocationRepository;
            EmploymentStatusRepository = employmentStatusRepository;
            EmploymentTypeRepository = employmentTypeRepository;
            PostRepository = postRepository;
            CostCenterRepository = costCenterRepository;
            GradeRepository = gradeRepository;
            JobLevelRepository = jobLevelRepository;
            JobTitleRepository = jobTitleRepository;
            LocationRepository = locationRepository;
            OrganizationUnitRepository = organizationUnitRepository;
            PostContactRepository = postContactRepository;
        }
       
        public IRepository<HRDbContext, Assignment, Guid> AssignmentRepository { get; }
        public IRepository<HRDbContext, Employment, Guid> EmploymentRepository { get; }
        public IRepository<HRDbContext, EmploymentLocation, Guid> EmploymentLocationRepository { get; }
        public IRepository<HRDbContext, EmploymentStatus, Guid> EmploymentStatusRepository { get; }
        public IRepository<HRDbContext, EmploymentType, Guid> EmploymentTypeRepository { get; }
        public IRepository<HRDbContext, Post, Guid> PostRepository { get; }
        public IRepository<HRDbContext, CostCenter, Guid> CostCenterRepository { get; }
        public IRepository<HRDbContext, Grade, Guid> GradeRepository { get; }
        public IRepository<HRDbContext, JobLevel, Guid> JobLevelRepository { get; }
        public IRepository<HRDbContext, JobTitle, Guid> JobTitleRepository { get; }
        public IRepository<HRDbContext, Location, Guid> LocationRepository { get; }
        public IRepository<HRDbContext, OrganizationUnit, Guid> OrganizationUnitRepository { get; }
        public IRepository<HRDbContext, PostContact, Guid> PostContactRepository { get; }
    }
}
