using Azure.Core;
using Core.Application.Abstractions;
using Core.Application.Abstractions.People;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Exporter.Excel;
using Core.Shared.Enums.HR;
using HR.Application.Commands.Employee;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Data;
using HR.IrisaSync.Extention.Contexts;
using HR.IrisaSync.Extention.Data;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Interface;
using HR.IrisaSync.Extention.Specifications;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Services
{
    public class SyncService : ISyncService
    {
        private readonly ISpecificationRepository<PdsIdeaInformationViw, string> _repoSpec;
        private readonly IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string> _irisaRepo;
        private readonly IHRUnitOfWork<HRDbContext> _hrUow;
        private readonly IIrisaSyncUnitOfWork<IrisaExtentionDbContext> _uow;
        private readonly IEmployeeInternalService _employeeService;
        private readonly IMapService _mapService;
        private readonly IPersonPublicService _personService;
        private readonly IMediator _mediator;
        public SyncService(ISpecificationRepository<PdsIdeaInformationViw, string> repoSpec,
            IHRUnitOfWork<HRDbContext> hrUow, IIrisaSyncUnitOfWork<IrisaExtentionDbContext> uow,
            IEmployeeInternalService employeeService,
            IPersonPublicService personService,
            IMapService mapService,
            IMediator mediator,
            IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string> irisaRepo)
        {
            _mapService = mapService;
            _mediator = mediator;
            _employeeService = employeeService;
            _irisaRepo = irisaRepo;
            _repoSpec = repoSpec;
            _uow = uow;
            _hrUow = hrUow;
            _personService = personService;
        }

        public async Task SyncEmployements()
        {
            var hrEmployees = await _hrUow.EmploymentRepository.GetAllAsync();
            var irisaEmployees = await _irisaRepo.GetAllAsync();
            // مسیر ذخیره‌سازی در ریشه درایو C
            string path = @"C:\output.xlsx";

            // فراخوانی متد
            ExcelExporter.ExportToExcel(irisaEmployees, path, "افراد");
            foreach (var item in irisaEmployees)
            {
                await syncEmployee(item);
            }

        }

        private async Task syncEmployee(PdsIdeaInformationViw item)
        {
            #region تعیین اینکه آیا کارمند جدید است یا از قبل وجود داشته است؟
            //var existEmp = await _uow.EmploymentRepository.GetByIdAsync()
            #endregion
            #region افزودن کارمند جدید
            
            //Guid postId = PostMapping.GetPostId(item.);
            Guid postId = Guid.NewGuid();
            var command = new CreateEmployeeCommand(
                 item.NumTelEmply.ToString(),
                item.DesAdrEmply,
                item.DesEmailAddresEmply,
                item.NumMobilEmply.ToString(),
                item.CodNatEmply,
                item.NamFirstEmply,
                item.NamLastEmply,
                Convert.ToDateTime(  item.DatBirthEmplyEn),
                item.BirthPlace,
                item.NamFathrEmply,
                item.DesSexEmply == "مذکر" ? Gender.Male : Gender.Female,
                item.NumPrsnEmply.ToString(),
                null ,
                null ,
                DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                null ,
                null ,
                postId ,
                PostAssignmentType.Delegation,
                DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                null 
                );

            var result = await _mediator.Send(command);
            
            #endregion
            #region ویرایش کارمند در صورت تغییر

            #endregion
        }

        public async Task<IReadOnlyList<PdsIdeaInformationViw>> GetEmployee()
        {
            var a = await _irisaRepo.GetByIdAsync("1250382831");
            var spec = new GetEmployeeSpec();
            var lst = await _repoSpec.ListBySpecAsync(spec);
            return lst.ToList();
        }


        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول های مپ و ویو ایریسا
        /// </summary>
        /// <returns></returns>
        public async Task SyncPost()
        {
            //IEnumerable<PdsIdeaInformationViw> irisaList = await _irisaRepo.GetAllAsync();
            var irisaList = (await _irisaRepo.GetAllAsync())
                     .Where(e => e.CodEmtyp == true)
                     .GroupBy(a => a.CodJobpo)
                     ;
            var jobTitleMapList = await _uow.JobTitleMapRepository.GetAllAsync();
            var jobLevelMapList = await _uow.JobLevelMapRepository.GetAllAsync();
            var organUnitMapList = await _uow.OrganizationUnitMapRepository.GetAllAsync();
            List<Post> posts = new List<Post>();

            foreach (var item in irisaList)
            {
                JobTitleMap jobTitle = jobTitleMapList.Where(j => j.IrisaJobTitleId == item.Key).SingleOrDefault();
                if (jobTitle?.FkJobTitleId != null)
                {
                    int counter = 0;
                    foreach (var grp in item.ToList())
                    {
                        counter++;
                        JobLevelMap? jobLevel = jobLevelMapList.Where(j => j.IrisaJobLevelId == grp.CodPosit).SingleOrDefault();
                        OrganizationUnitMap? orgunit = organUnitMapList.Where(j => j.IrisaOrganizationUnitId == grp.CodBusun).SingleOrDefault();
                        posts.Add(new Post(
                            counter.ToString(),
                            (Guid)jobTitle.FkJobTitleId,
                            orgunit?.FkOrganizationUnitId,
                            jobLevel?.FkJobLevelId

                            ));
                    }
                }
            }
            await _hrUow.PostRepository.AddRangeAsync(posts);
            await _hrUow.SaveChangesAsync();

        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task SyncJobTitle()
        {
            var list = (await _uow.JobTitleMapRepository.GetAllAsync()).Where(e => e.FkJobTitleId == null);

            foreach (var item in list)
            {
                if (item.IrisaJobTitle != null)
                {
                    JobTitle model = new JobTitle(item.IrisaJobTitleId.ToString(), item.IrisaJobTitle);
                    await _hrUow.JobTitleRepository.AddAsync(model);
                    item.FkJobTitleId = model.Id;
                    item.JobTitle = model.Name;
                    await _uow.JobTitleMapRepository.UpdateAsync(item);
                }
            }

            await _hrUow.SaveChangesAsync();
            await _uow.SaveChangesAsync();
        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task SyncJobLevel()
        {
            var list = (await _uow.JobLevelMapRepository.GetAllAsync()).Where(e => e.FkJobLevelId == null);

            foreach (var item in list)
            {
                if (item.IrisaJobLevel != null)
                {
                    JobLevel model = new JobLevel(item.IrisaJobLevelId.ToString(), item.IrisaJobLevel);
                    await _hrUow.JobLevelRepository.AddAsync(model);
                    item.FkJobLevelId = model.Id;
                    item.JobLevel = model.Title;
                    await _uow.JobLevelMapRepository.UpdateAsync(item);
                }
            }

            await _hrUow.SaveChangesAsync();
            await _uow.SaveChangesAsync();

        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task SyncOrganizationUnit()
        {
            var list = (await _uow.OrganizationUnitMapRepository.GetAllAsync()).Where(e => e.FkOrganizationUnitId == null);
            var existList = await _hrUow.OrganizationUnitRepository.GetAllAsync();
            foreach (var item in list)
            {
                if (item.IrisaOrganizationUnit != null)
                {
                    var existEntity = existList.Where(a => a.Id == item.FkOrganizationUnitId).SingleOrDefault();
                    if (existEntity != null)
                    {
                      
                    }
                    else
                    {
                        OrganizationUnit model = new OrganizationUnit(item.IrisaOrganizationUnit, item.IrisaOrganizationUnitId.ToString(), null);
                        await _hrUow.OrganizationUnitRepository.AddAsync(model);
                        item.FkOrganizationUnitId = model.Id;
                        item.OrganizationUnit = model.Name;
                        await _uow.OrganizationUnitMapRepository.UpdateAsync(item);
                    }
                }
            }

            await _hrUow.SaveChangesAsync();
            await _uow.SaveChangesAsync();

        }
    }
}
