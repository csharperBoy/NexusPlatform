using Core.Application.Abstractions;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface IHRUnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : DbContext
    {
         IRepository<TContext,Assignment , Guid> AssignmentRepository { get; }
         IRepository<TContext, Employment, Guid> EmploymentRepository { get; }
         IRepository<TContext, EmploymentLocation, Guid> EmploymentLocationRepository { get; }
         IRepository<TContext, EmploymentStatus, Guid> EmploymentStatusRepository { get; }
         IRepository<TContext, EmploymentType, Guid> EmploymentTypeRepository { get; }
         IRepository<TContext, Post, Guid> PostRepository { get; }
         IRepository<TContext, PostLocation, Guid> PostLocationRepository { get; }
         IRepository<TContext, CostCenter, Guid> CostCenterRepository { get; }
         IRepository<TContext, Grade, Guid> GradeRepository { get; }
         IRepository<TContext, JobLevel, Guid> JobLevelRepository { get; }
         IRepository<TContext, JobTitle, Guid> JobTitleRepository { get; }
         IRepository<TContext, Location, Guid> LocationRepository { get; }
         //IRepository<TContext, LocationContact, Guid> LocationContactRepository { get; }
         IRepository<TContext, OrganizationUnit, Guid> OrganizationUnitRepository { get; }
         //IRepository<TContext, PostContact, Guid> PostContactRepository { get; }
         IRepository<TContext, EmploymentInfoView, Guid> EmployementInfoViewRepository { get; }
         IRepository<TContext, PostInfoView, Guid> PostInfoViewRepository { get; }
        
    }
}
