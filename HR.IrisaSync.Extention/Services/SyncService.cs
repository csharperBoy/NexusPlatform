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
using System.Transactions;

namespace HR.IrisaSync.Extention.Services
{
    public class SyncResult
    {
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int DeletedCount { get; set; }

        public override string ToString()
            => $"Added: {AddedCount}, Updated: {UpdatedCount}, Deleted: {DeletedCount}";
    }
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
            //var hrEmployees = await _hrUow.EmploymentRepository.GetAllAsync();
            var irisaEmployees = (await _irisaRepo.GetAllAsync()).Where(a => a.CodEmtyp == true && a.NumPrsnEmply == 310);
            
            foreach (var item in irisaEmployees)
            {
                //await syncEmployee(item);
            }

        }
        public async Task<SyncResult> SyncEmployeesAsync()
        {
            var result = new SyncResult();

            //using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // 1. دریافت کارمندان از ویو خارجی (فیلتر شده)
                var irisaEmployees = (await _irisaRepo.GetAllAsync())
                    .Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null)
                    .ToList();

                // 2. دریافت مپ عنوان‌های شغلی
                var jobTitleMap = (await _uow.JobTitleMapRepository.GetAllAsync()).Where(a=>a.IrisaJobTitleId!=null)
                    .ToDictionary(j => j.IrisaJobTitleId, j => j.FkJobTitleId);

                // 3. دریافت تمام پست‌های موجود و ساخت دیکشنری (JobTitleId, Code) -> Post
                var allPosts = await _hrUow.PostRepository.GetAllAsync();
                var postDict = allPosts
                    .Where(p => p.FkJobTitleId != Guid.Empty && !string.IsNullOrEmpty(p.Code))
                    .ToDictionary(
                        p => (p.FkJobTitleId, p.Code),
                        p => p
                    );

                // 4. دریافت تمام کارمندان موجود در دیتابیس (برای تشخیص جدید/موجود)
                var existingEmployees = await _hrUow.EmploymentRepository.GetAllAsync();
                var employeeDict = existingEmployees
                    .ToDictionary(e => e.EmployeeCode, e => e); // PersonalCode = NumPrsnEmply

                // 5. لیست عملیات (برای رهگیری)
                var employeesToUpdate = new List<Employment>();
                var employeesToDelete = new List<Employment>();

                // 6. گروه‌بندی کارمندان ویو بر اساس CodJobpo
                var employeeGroups = irisaEmployees.Where(a=>a.CodJobpo != null)
                    .GroupBy(e => e.CodJobpo)
                    .ToList();

                foreach (var group in employeeGroups)
                {
                    // یافتن FkJobTitleId معتبر
                    if (!jobTitleMap.TryGetValue(group.Key, out var jobTitleId))
                        continue; // اگر عنوان شغلی مپ نشده، گروه را نادیده بگیر

                    // مرتب‌سازی کارمندان گروه بر اساس یک ترتیب مشخص (مثلاً NumPrsnEmply)
                    var sortedEmployees = group
                        .OrderBy(e => e.NumPrsnEmply) // یا هر فیلد دیگری مانند تاریخ استخدام
                        .ToList();

                    int counter = 0;
                    foreach (var item in sortedEmployees)
                    {
                        counter++;
                        string code = counter.ToString();
                        var key = (JobTitleId: (Guid)jobTitleId, Code: code);

                        // پیدا کردن پست متناظر
                        if (!postDict.TryGetValue(key, out var post))
                        {
                            // اگر پست وجود نداشت، خطا ثبت کن یا ادامه بده
                            // _logger.LogWarning($"پستی با عنوان شغلی {jobTitleId} و کد {code} یافت نشد.");
                            continue;
                        }

                        var personalCode = item.NumPrsnEmply.ToString();
                        var postId = post.Id;

                        // 7. بررسی وجود کارمند در دیتابیس
                        if (employeeDict.TryGetValue(personalCode, out var existingEmployee))
                        {
                            // ➡️ کارمند موجود است → به‌روزرسانی از طریق MediatR
                           /* var updateCommand = new UpdateEmployeeCommand(
                                // پارامترهای مورد نیاز برای به‌روزرسانی
                                // (همان فیلدهای CreateEmployeeCommand به اضافه Id یا PersonalCode)
                                PersonalCode: personalCode,
                                FirstName: item.NamFirstEmply,
                                LastName: item.NamLastEmply,
                                // ... سایر فیلدها
                                PostId: postId,
                                // ...
                            );

                            var updateResult = await _mediator.Send(updateCommand);*/
                            // در صورت موفقیت، تعداد به‌روز شده را افزایش بده
                            result.UpdatedCount++;

                            // حذف از دیکشنری تا بعداً متوجه شویم کدام کارمندها حذف می‌شوند
                            employeeDict.Remove(personalCode);
                        }
                        else
                        {
                            // ➕ کارمند جدید → ایجاد از طریق MediatR
                            var createCommand = new CreateEmployeeCommand(
                                Phone : item.NumTelEmply.ToString(),
                                Address: item.DesAdrEmply,
                                Email: null, // یا item.DesEmailAddresEmply
                                Mobile: item.NumMobilEmply.ToString(),
                                NationalCode: item.CodNatEmply,
                                FirstlName: item.NamFirstEmply,
                                LastName: item.NamLastEmply,
                                BirthDate: Convert.ToDateTime(item.DatBirthEmplyEn),
                                BirthPlace: item.BirthPlace,
                                FatherName: item.NamFathrEmply,
                                Gender: item.DesSexEmply.Trim() == "مذکر" ? Gender.Male : Gender.Female,
                                EmployeeCode: personalCode,
                                StartDate: DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                                PostId: postId,
                                AssigneeType: PostAssignmentType.Delegation,
                                EffectiveFrom: DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                                EffectiveTo: null,EmploymentStatusId: null , EmploymentTypeId: null,EndDate: null,locationsId:null
                            );

                            var createResult = await _mediator.Send(createCommand);
                            result.AddedCount++;
                        }
                    }
                }

                // 8. کارمندانی که در دیکشنری باقی مانده‌اند = در ویو نیستند → باید حذف یا غیرفعال شوند
                employeesToDelete = employeeDict.Values.ToList();

                foreach (var emp in employeesToDelete)
                {
                    // فرض کنید یک Command برای حذف یا غیرفعال‌سازی دارید
                  /*  var deleteCommand = new DeactivateEmployeeCommand(emp.Id);
                    await _mediator.Send(deleteCommand);*/
                    result.DeletedCount++;
                }

                //scope.Complete();
                return result;
            }
            catch (Exception ex)
            {
                // لاگ خطا
                // _logger.LogError(ex, "خطا در سینک کارمندان");
                throw;
            }
        }
        /*
        private async Task syncEmployee(PdsIdeaInformationViw item)
        {
            #region تعیین اینکه آیا کارمند جدید است یا از قبل وجود داشته است؟
            //var existEmp = await _uow.EmploymentRepository.GetByIdAsync();
            #endregion
            #region افزودن کارمند جدید
           
            Guid postId = 
            var command = new CreateEmployeeCommand(
                 item.NumTelEmply.ToString(),
                item.DesAdrEmply,
                //item.DesEmailAddresEmply,
                null,
                item.NumMobilEmply.ToString(),
                item.CodNatEmply,
                item.NamFirstEmply,
                item.NamLastEmply,
                Convert.ToDateTime(item.DatBirthEmplyEn),
                item.BirthPlace,
                item.NamFathrEmply,
                item.DesSexEmply.Trim() == "مذکر" ? Gender.Male : Gender.Female,
                item.NumPrsnEmply.ToString(),
                null,
                null,
                DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                null,
                null,
                postId,
                PostAssignmentType.Delegation,
                DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                null
                );

            var result = await _mediator.Send(command);

            #endregion
            #region ویرایش کارمند در صورت تغییر

            #endregion
        }
        */
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
        /*  public async Task SyncPost2()
          {
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
          */
        //public async Task<SyncResult> SyncPostAssignToEmployementAsync()
        //{
        //    var result = new SyncResult();
        //    var irisaGroups = (await _irisaRepo.GetAllAsync())
        //           .Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null)
        //           .ToList();
        //}
        public async Task<SyncResult> SyncPostAsync()
        {
            var result = new SyncResult();

            // استفاده از TransactionScope برای اتمیک بودن
            //using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // 1. دریافت داده‌های خارجی و گروه‌بندی
                var irisaGroups = (await _irisaRepo.GetAllAsync())
                    .Where(e => e.CodEmtyp == true && e.CodJobpo != null)
                    .GroupBy(a => a.CodJobpo)
                    .ToList();

                // 2. دریافت مپ‌ها به صورت دیکشنری برای جستجوی O(1)
                var jobTitleMap = (await _uow.JobTitleMapRepository.GetAllAsync()).Where(a => a.IrisaJobTitleId != null)
                    .ToDictionary(j => j.IrisaJobTitleId, j => j.FkJobTitleId);

                var jobLevelMap = (await _uow.JobLevelMapRepository.GetAllAsync()).Where(a => a.IrisaJobLevelId != null)
                    .ToDictionary(j => j.IrisaJobLevelId, j => j.FkJobLevelId);

                var organUnitMap = (await _uow.OrganizationUnitMapRepository.GetAllAsync()).Where(a => a.IrisaOrganizationUnitId != null)
                    .ToDictionary(j => j.IrisaOrganizationUnitId, j => j.FkOrganizationUnitId);

                // 3. دریافت پست‌های موجود (فقط فیلدهای لازم)
                var existingPosts = await _hrUow.PostRepository
                    .GetAllAsync(); // اگر IQueryable هست، بهتر است Select کنید

                // 4. ساخت دیکشنری از پست‌های موجود با کلید (JobTitleId, Code)
                var existingDict = existingPosts
                    .Where(p => p.FkJobTitleId != Guid.Empty && !string.IsNullOrEmpty(p.Code))
                    .ToDictionary(
                        p => (p.FkJobTitleId, p.Code),
                        p => p
                    );

                // 5. مجموعه کلیدهای جدید برای تشخیص پست‌های حذفی
                var newKeys = new HashSet<(Guid JobTitleId, string Code)>();

                // 6. لیست عملیات
                var postsToAdd = new List<Post>();
                var postsToUpdate = new List<Post>();

                // 7. پردازش هر گروه عنوان شغلی
                foreach (var group in irisaGroups)
                {
                    // پیدا کردن عنوان شغلی معتبر
                    if (!jobTitleMap.TryGetValue(group.Key, out var jobTitleId))
                        continue; // اگر مپ وجود ندارد، کل گروه را نادیده بگیر

                    int counter = 0;
                    foreach (var item in group)
                    {
                        counter++;
                        string code = counter.ToString();

                        // دریافت مقادیر مپ‌شده (در صورت وجود)
                        jobLevelMap.TryGetValue(item.CodPosit, out var jobLevelId);
                        organUnitMap.TryGetValue(item.CodBusun, out var orgUnitId);

                        var key = (JobTitleId: (Guid)jobTitleId, Code: code);
                        newKeys.Add(key);

                        if (existingDict.TryGetValue(key, out var existingPost))
                        {
                            // ➡️ پست موجود است – بررسی تغییرات و به‌روزرسانی
                            bool hasChanges = false;

                            if (existingPost.FkOrganizationUnitId != orgUnitId ||
                                existingPost.FkJobLevelId != jobLevelId)
                            {
                                // از متد UpdateDetails استفاده می‌کنیم
                                existingPost.UpdateDetails(
                                    organizationUnitId: orgUnitId,
                                    jobLevelId: jobLevelId,
                                    gradeId: null,      // در صورت نیاز
                                    costCenterId: null, // در صورت نیاز
                                    parentId: null      // در صورت نیاز
                                );
                                hasChanges = true;
                            }

                            // در صورت تغییر، به لیست به‌روز اضافه کن
                            if (hasChanges)
                                postsToUpdate.Add(existingPost);

                            // حذف از دیکشنری تا بعداً متوجه بشیم کدوم پست‌ها حذف شدن
                            existingDict.Remove(key);
                        }
                        else
                        {
                            // ➕ پست جدید
                            var newPost = new Post(
                                _Code: code,
                                _JobTitleId: (Guid)jobTitleId,
                                _OrganizationUnitId: orgUnitId,
                                _JobLevelId: jobLevelId,
                                _GradeId: null,
                                _CostCenterId: null,
                                _parentId: null
                            );
                            postsToAdd.Add(newPost);
                        }
                    }
                }

                // 8. پست‌های باقی‌مانده در دیکشنری = باید حذف شوند
                var postsToDelete = existingDict.Values.ToList();

                // 9. اعمال تغییرات روی دیتابیس
                if (postsToAdd.Any())
                {
                    await _hrUow.PostRepository.AddRangeAsync(postsToAdd);
                    result.AddedCount = postsToAdd.Count;
                }

                if (postsToUpdate.Any())
                {
                    await _hrUow.PostRepository.UpdateRangeAsync(postsToUpdate);
                    result.UpdatedCount = postsToUpdate.Count;
                }

                if (postsToDelete.Any())
                {
                    await _hrUow.PostRepository.RemoveRangeAsync(postsToDelete);
                    result.DeletedCount = postsToDelete.Count;
                }

                // ذخیره‌سازی نهایی
                await _hrUow.SaveChangesAsync();

                // تکمیل تراکنش
                //scope.Complete();

                return result;
            }
            catch (Exception ex)
            {
                // لاگ خطا (در صورت وجود ILogger)
                // _logger.LogError(ex, "خطا در سینک پست‌ها");
                throw; // یا بازگرداندن یک نتیجه با خطا
            }
        }

        // کلاس نتیجه


        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task SyncJobTitle()
        {
            var list = await _uow.JobTitleMapRepository.GetAllAsync();
            var existList = await _hrUow.JobTitleRepository.GetAllAsync();

            foreach (var item in list)
            {
                if (item.IrisaJobTitle != null)
                {
                    if (item.IrisaJobTitle != null)
                    {
                        var existEntity = existList.Where(a => a.Id == item.FkJobTitleId).SingleOrDefault();
                        if (existEntity != null)
                        {
                            if (existEntity.Name.Trim() != item.JobTitle.Trim())
                            {
                                existEntity.SetName(item.JobTitle);
                                await _hrUow.JobTitleRepository.UpdateAsync(existEntity);
                            }
                        }
                        else
                        {
                            JobTitle model = new JobTitle(item.IrisaJobTitleId.ToString(), item.IrisaJobTitle);
                            await _hrUow.JobTitleRepository.AddAsync(model);
                            item.FkJobTitleId = model.Id;
                            item.JobTitle = model.Name;
                            await _uow.JobTitleMapRepository.UpdateAsync(item);
                        }
                    }
                }

                await _hrUow.SaveChangesAsync();
                await _uow.SaveChangesAsync();
            }
        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task SyncJobLevel()
        {
            var list = await _uow.JobLevelMapRepository.GetAllAsync();
            var existList = await _hrUow.JobLevelRepository.GetAllAsync();

            foreach (var item in list)
            {
                if (item.IrisaJobLevel != null)
                {
                    if (item.IrisaJobLevel != null)
                    {
                        var existEntity = existList.Where(a => a.Id == item.FkJobLevelId).SingleOrDefault();
                        if (existEntity != null)
                        {
                            if (existEntity.Title.Trim() != item.JobLevel.Trim())
                            {
                                existEntity.SetTitle(item.JobLevel);
                                await _hrUow.JobLevelRepository.UpdateAsync(existEntity);
                            }
                        }
                        else
                        {
                            JobLevel model = new JobLevel(item.IrisaJobLevelId.ToString(), item.IrisaJobLevel);
                            await _hrUow.JobLevelRepository.AddAsync(model);
                            item.FkJobLevelId = model.Id;
                            item.JobLevel = model.Title;
                            await _uow.JobLevelMapRepository.UpdateAsync(item);
                        }
                    }
                }

                await _hrUow.SaveChangesAsync();
                await _uow.SaveChangesAsync();

            }
        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task SyncOrganizationUnit()
        {
            var list = await _uow.OrganizationUnitMapRepository.GetAllAsync();
            var existList = await _hrUow.OrganizationUnitRepository.GetAllAsync();
            foreach (var item in list)
            {
                if (item.IrisaOrganizationUnit != null)
                {
                    var existEntity = existList.Where(a => a.Id == item.FkOrganizationUnitId).SingleOrDefault();
                    if (existEntity != null)
                    {
                        if (existEntity.Name.Trim() != item.OrganizationUnit.Trim())
                        {
                            existEntity.SetName(item.OrganizationUnit);
                            await _hrUow.OrganizationUnitRepository.UpdateAsync(existEntity);
                        }
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
