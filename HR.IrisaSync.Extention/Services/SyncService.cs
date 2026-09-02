using Azure.Core;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Exporter.Excel;
using Core.Shared.Enums.HR;
using DocumentFormat.OpenXml.Office.CustomUI;
using HR.Application.Commands.Employment;
using HR.Application.Commands.OrgChart;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Events.Employment;
using HR.Domain.Events.Post;
using HR.Infrastructure.Data;
using HR.Infrastructure.Services;
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
        public List<string> Errors { get; set; } = new List<string>();
        public override string ToString()
            => $"Added: {AddedCount}, Updated: {UpdatedCount}, Deleted: {DeletedCount}";
    }
    public class SyncService : ISyncService
    {
        private readonly ISpecificationRepository<PdsIdeaInformationViw, string> _repoSpec;
        private readonly IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string> _irisaRepo;
        private readonly IHRUnitOfWork<HRDbContext> _hrUow;
        private readonly IIrisaSyncUnitOfWork<IrisaExtentionDbContext> _uow;
        private readonly IEmploymentInternalService _employmentService;
        private readonly IPostInternalService _postService;
        private readonly IMapService _mapService;
        private readonly IPersonPublicService _personService;
        private readonly IContactPublicService _contactService;
        private readonly IMediator _mediator;
        public SyncService(ISpecificationRepository<PdsIdeaInformationViw, string> repoSpec,
            IHRUnitOfWork<HRDbContext> hrUow, IIrisaSyncUnitOfWork<IrisaExtentionDbContext> uow,
            IEmploymentInternalService employmentService,
            IPostInternalService postService,
            IPersonPublicService personService,
            IContactPublicService contactService,
            IMapService mapService,
            IMediator mediator,
            IRepository<IrisaOracleDbContext, PdsIdeaInformationViw, string> irisaRepo)
        {
            _mapService = mapService;
            _mediator = mediator;
            _employmentService = employmentService;
            _postService = postService;
            _contactService = contactService;
            _irisaRepo = irisaRepo;
            _repoSpec = repoSpec;
            _uow = uow;
            _hrUow = hrUow;
            _personService = personService;
        }
        /*
        public async Task<SyncResult> SyncEmploymentsAsync2()
        {
            var result = new SyncResult();

            try
            {
                // 1. دریافت کارمندان از ویو خارجی (فیلتر شده)
                var irisaEmployments = (await _irisaRepo.GetAllAsync())
                    .Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null)
                    .ToList();

                // 2. دریافت مپ عنوان‌های شغلی
                var jobTitleMap = (await _uow.JobTitleMapRepository.GetAllAsync()).Where(a => a.IrisaJobTitleId != null)
                    .ToDictionary(j => j.IrisaJobTitleId, j => j.FkJobTitleId);

                // 3. دریافت تمام پست‌های موجود و ساخت دیکشنری (JobTitleId, Code) -> Post
                var allPosts = await _hrUow.PostRepository.GetAllAsync();
                var postDict = allPosts
                    .Where(p => p.FkJobTitleId != Guid.Empty && !string.IsNullOrEmpty(p.Code) && p.IsRemove != true)
                    .ToDictionary(
                        p => (p.FkJobTitleId, p.Code),
                        p => p
                    );

                // 4. دریافت تمام کارمندان موجود در دیتابیس (برای تشخیص جدید/موجود)
                var existingEmployments = await _hrUow.EmploymentRepository.GetAllAsync();
                var employmentDict = existingEmployments
                    .ToDictionary(e => e.EmploymentCode, e => e); // PersonalCode = NumPrsnEmply

                // 5. لیست عملیات (برای رهگیری)
                var employmentsToUpdate = new List<Employment>();
                var employmentsToDelete = new List<Employment>();

                // 6. گروه‌بندی کارمندان ویو بر اساس CodJobpo
                var employmentGroups = irisaEmployments.Where(a => a.CodJobpo != null)
                    .GroupBy(e => e.CodJobpo)
                    .ToList();

                foreach (var group in employmentGroups)
                {
                    // یافتن FkJobTitleId معتبر
                    if (!jobTitleMap.TryGetValue(group.Key, out var jobTitleId))
                        continue; // اگر عنوان شغلی مپ نشده، گروه را نادیده بگیر

                    // مرتب‌سازی کارمندان گروه بر اساس یک ترتیب مشخص (مثلاً NumPrsnEmply)
                    var sortedEmployments = group
                        .OrderBy(e => e.NumPrsnEmply) // یا هر فیلد دیگری مانند تاریخ استخدام
                        .ToList();

                    int counter = 0;
                    foreach (var item in sortedEmployments)
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
                        if (employmentDict.TryGetValue(personalCode, out var existingEmployment))
                        {
                            UpdateEmploymentCommand updateCommand = new UpdateEmploymentCommand(
                                Id: existingEmployment.Id,
                                Phone: new List<string> { item.NumTelEmply.ToString() },
                                Address: new List<string> { item.DesAdrEmply },
                                Mobile: new List<string> { item.NumMobilEmply.ToString() },
                                nationalCode: item.CodNatEmply,
                                FirstName: item.NamFirstEmply,
                                LastName: item.NamLastEmply,
                                BirthDate: Convert.ToDateTime(item.DatBirthEmplyEn),
                                BirthPlace: item.BirthPlace,
                                FatherName: item.NamFathrEmply,
                                EmploymentCode: personalCode,
                                StartDate: DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                                PostId: postId,
                                EffectiveFrom: Convert.ToDateTime(item.DatEmpltEmplyEn)

                                );

                            var updateResult = await _mediator.Send(updateCommand);
                            //await _postService.AssignToEmploymentAsync(new List<Guid?> { postId }, existingEmployment.Id);
                            // در صورت موفقیت، تعداد به‌روز شده را افزایش بده
                            result.UpdatedCount++;
                            existingEmployment.AddDomainEvent(new ChangeEmploymentEvent(existingEmployment.Id));
                            // حذف از دیکشنری تا بعداً متوجه شویم کدام کارمندها حذف می‌شوند
                            employmentDict.Remove(personalCode);
                        }
                        else
                        {

                            // ➕ کارمند جدید → ایجاد از طریق MediatR
                            var createCommand = new CreateEmploymentCommand(
                                Phone: new List<string> { item.NumTelEmply.ToString() },
                                Address: new List<string> { item.DesAdrEmply },
                                Email: null, // یا item.DesEmailAddresEmply
                                Mobile: new List<string> { item.NumMobilEmply.ToString() },
                                OfficePhone: null,
                                OrgEmail: null,
                                OrgMobile: null,
                                NationalCode: item.CodNatEmply,
                                FirstlName: item.NamFirstEmply,
                                LastName: item.NamLastEmply,
                                BirthDate: Convert.ToDateTime(item.DatBirthEmplyEn),
                                BirthPlace: item.BirthPlace,
                                FatherName: item.NamFathrEmply,
                                Gender: item.DesSexEmply.Trim() == "مذکر" ? Gender.Male : Gender.Female,
                                EmploymentCode: personalCode,
                                StartDate: DateOnly.FromDateTime(Convert.ToDateTime(item.DatEmpltEmplyEn)),
                                PostId: postId,
                                AssigneeType: PostAssignmentType.Delegation,
                                EffectiveFrom: Convert.ToDateTime(item.DatEmpltEmplyEn),
                                EffectiveTo: null, EmploymentStatusId: null, EmploymentTypeId: null, EndDate: null, locationsId: null
                            );

                            var createResult = await _mediator.Send(createCommand);
                            result.AddedCount++;
                        }
                    }
                }

                // 8. کارمندانی که در دیکشنری باقی مانده‌اند = در ویو نیستند → باید حذف یا غیرفعال شوند
                employmentsToDelete = employmentDict.Values.ToList();

                foreach (var emp in employmentsToDelete)
                {
                    // فرض کنید یک Command برای حذف یا غیرفعال‌سازی دارید
                    var deleteCommand = new DeleteEmploymentCommand(emp.Id);
                    await _mediator.Send(deleteCommand);
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
        */
        public async Task<SyncResult> SyncEmploymentsAsync()
        {
            var result = new SyncResult();

            try
            {
                // 1. دریافت کارمندان از ویو خارجی (فیلتر شده)
                List<PdsIdeaInformationViw> externalEmployments = (await _irisaRepo.GetAllAsync())
                    .Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null)
                    .ToList();

                // 2. دریافت تمام کارمندان موجود در دیتابیس
                IEnumerable<EmploymentInfoView> existingEmployments = await _hrUow.EmployementInfoViewRepository.GetAllAsync();

                // ساخت دیکشنری با کلید ترکیبی (کدپرسنلی + کدملی) برای دسترسی سریع
                var existingDict = existingEmployments
                    .ToDictionary(
                        e => $"{e.EmploymentCode}_{e.NationalCode}",
                        e => e
                    );

                // 3. دسته‌بندی رکوردها
                var toAdd = new List<PdsIdeaInformationViw>();
                var toUpdate = new List<(PdsIdeaInformationViw External, EmploymentInfoView Existing)>();

                foreach (var ext in externalEmployments)
                {
                    string personalCode = ext.NumPrsnEmply.ToString();
                    string nationalCode = ext.CodNatEmply;
                    string key = $"{personalCode}_{nationalCode}";

                    if (existingDict.TryGetValue(key, out var existing))
                    {
                        toUpdate.Add((ext, existing));
                        existingDict.Remove(key);
                    }
                    else
                    {
                        toAdd.Add(ext);
                    }
                }

                // کارمندانی که باید حذف شوند
                var toDelete = existingDict.Values.ToList();


                // ============ 5. عملیات افزودن (تکی) ============
                foreach (var ext in toAdd)
                {
                    try
                    {


                        var createCommand = new CreateEmploymentCommand(
                            Phone: new List<string> { ext.NumTelEmply.ToString() },
                            Address: new List<string> { ext.DesAdrEmply },
                            Email: null,
                            Mobile: new List<string> { ext.NumMobilEmply.ToString() },
                            OfficePhone: null,
                            OrgEmail: null,
                            OrgMobile: null,
                            NationalCode: ext.CodNatEmply,
                            FirstlName: ext.NamFirstEmply,
                            LastName: ext.NamLastEmply,
                            BirthDate: Convert.ToDateTime(ext.DatBirthEmplyEn),
                            BirthPlace: ext.BirthPlace,
                            FatherName: ext.NamFathrEmply,
                            Gender: ext.DesSexEmply.Trim() == "مذکر" ? Gender.Male : Gender.Female,
                            EmploymentCode: ext.NumPrsnEmply.ToString(),
                            StartDate: DateOnly.FromDateTime(Convert.ToDateTime(ext.DatEmpltEmplyEn)),
                            PostId: null,
                            AssigneeType: null,
                            EffectiveFrom: Convert.ToDateTime(ext.DatEmpltEmplyEn),
                            EffectiveTo: null,
                            EmploymentStatusId: null,
                            EmploymentTypeId: null,
                            EndDate: null,
                            locationsId: null
                        );

                        await _mediator.Send(createCommand);
                        result.AddedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error adding {ext.NumPrsnEmply}: {ex.Message}");
                    }
                }

                // ============ 6. عملیات بروزرسانی (دسته‌جمعی) ============
                if (toUpdate.Any())
                {
                    var updateCommands = new List<UpdateEmploymentCommand>();

                    foreach (var (ext, existing) in toUpdate)
                    {
                        try
                        {
                            string personalCode = ext.NumPrsnEmply.ToString();

                            var command = new UpdateEmploymentCommand(
                                Id: existing.Id,
                                Phone: new List<string> { ext.NumTelEmply.ToString() },
                                Address: new List<string> { ext.DesAdrEmply },
                                Mobile: new List<string> { ext.NumMobilEmply.ToString() },
                                nationalCode: ext.CodNatEmply,
                                FirstName: ext.NamFirstEmply,
                                LastName: ext.NamLastEmply,
                                BirthDate: Convert.ToDateTime(ext.DatBirthEmplyEn),
                                BirthPlace: ext.BirthPlace,
                                FatherName: ext.NamFathrEmply,
                                EmploymentCode: personalCode,
                                StartDate: DateOnly.FromDateTime(Convert.ToDateTime(ext.DatEmpltEmplyEn)),
                                PostId: Optional<Guid?>.Undefined,
                                EffectiveFrom: Convert.ToDateTime(ext.DatEmpltEmplyEn)
                            );

                            updateCommands.Add(command);
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error preparing update for {ext.NumPrsnEmply}: {ex.Message}");
                        }
                    }

                    if (updateCommands.Any())
                    {
                        var batchUpdateCommand = new BatchUpdateEmploymentsCommand(updateCommands);
                        var batchResult = await _mediator.Send(batchUpdateCommand);

                        result.UpdatedCount = updateCommands.Count;
                    }
                }

                // ============ 7. عملیات حذف (تکی) ============
                foreach (var emp in toDelete)
                {
                    try
                    {
                        var deleteCommand = new DeleteEmploymentCommand(emp.Id);
                        await _mediator.Send(deleteCommand);
                        result.DeletedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error deleting {emp.EmploymentCode}: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                result.Errors.Add($"Sync failed: {ex.Message}");
            }

            return result;
        }


        public async Task<IReadOnlyList<PdsIdeaInformationViw>> GetEmployment()
        {
            var spec = new GetEmploymentSpec();
            var lst = await _repoSpec.ListBySpecAsync(spec);
            return lst.ToList();
        }


        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول های مپ و ویو ایریسا
        /// </summary>
        /// <returns></returns>
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
                    .GetAllAsync(queryOptions: q => q.Where(a => a.IsRemove != true)); // اگر IQueryable هست، بهتر است Select کنید

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
                //var postsToAdd = new List<Post>();
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
                                UpdatePostCommand updateCommand = new UpdatePostCommand(
                                    Id: existingPost.Id,
                                    OrganizationUnitId: orgUnitId,
                                    JobLevelId: jobLevelId
                                );
                                // از متد UpdateDetails استفاده می‌کنیم
                                //existingPost.UpdateDetails(
                                //    organizationUnitId: orgUnitId,
                                //    jobLevelId: jobLevelId,
                                //    gradeId: null,      // در صورت نیاز
                                //    costCenterId: null, // در صورت نیاز
                                //    parentId: null      // در صورت نیاز
                                //);

                                var createResult = await _mediator.Send(updateCommand);
                                hasChanges = true;
                            }

                            // در صورت تغییر، به لیست به‌روز اضافه کن
                            if (hasChanges)
                            {
                                postsToUpdate.Add(existingPost);
                            }
                            // حذف از دیکشنری تا بعداً متوجه بشیم کدوم پست‌ها حذف شدن
                            existingDict.Remove(key);
                        }
                        else
                        {

                            //Guid contactProfileId = await _contactService.CreateContactProfileAsync($"Post - {code}", ContactProfileTypeEnum.Post);
                            // ➕ پست جدید
                            var createCommand = new CreatePostCommand(
                                code,
                                 (Guid)orgUnitId,
                                 (Guid)jobTitleId,
                                 jobLevelId,
                                 null,
                                 null,
                                 null,
                                 true,
                                 null,
                                 null,
                                 null,
                                 null,
                                 null,
                                 null
                            );

                            var createResult = await _mediator.Send(createCommand);
                            result.AddedCount++;
                            //postsToAdd.Add(newPost);
                        }
                    }
                }

                // 8. پست‌های باقی‌مانده در دیکشنری = باید حذف شوند
                var postsToDelete = existingDict.Values.ToList();

                // 9. اعمال تغییرات روی دیتابیس
                //if (postsToAdd.Any())
                //{
                //    await _hrUow.PostRepository.AddRangeAsync(postsToAdd);
                //    result.AddedCount = postsToAdd.Count;
                //}

                if (postsToUpdate.Any())
                {
                    BatchUpdatePostsCommand updateCommand = new BatchUpdatePostsCommand(postsToUpdate.Select(a => new UpdatePostCommand(
                        a.Id, a.Code, a.FkOrganizationUnitId, a.FkJobTitleId, a.FkJobLevelId,
                        Optional<Guid?>.Undefined,
                        Optional<Guid?>.Undefined,
                        Optional<Guid?>.Undefined,
                        Optional<bool?>.Undefined,
                        Optional<Guid?>.Undefined,
                        Optional<PostAssignmentType?>.Undefined,
                        Optional<List<Guid>?>.Undefined,
                        Optional<List<string>?>.Undefined,
                        Optional<List<string>?>.Undefined,
                        Optional<List<string>?>.Undefined
                        )).ToList());

                    var createResult = await _mediator.Send(updateCommand);
                    //await _hrUow.PostRepository.UpdateRangeAsync(postsToUpdate);

                    //foreach (var post in postsToUpdate)
                    //{
                    //    post.AddDomainEvent(new ChangePostEvent(post.Id));
                    //}
                    result.UpdatedCount = postsToUpdate.Count;
                }

                if (postsToDelete.Any())
                {
                    //foreach (var item in postsToDelete)
                    //{
                    //    await item.SoftRemove();
                    //    foreach (var ass in item.Assignments)
                    //    {
                    //        ass.DoExpire();
                    //    }
                    //}
                    //foreach (var post in postsToDelete)
                    //{

                    //    post.AddDomainEvent(new ChangePostEvent(post.Id));
                    //}
                    foreach (var post in postsToDelete)
                    {
                        DeletePostCommand deleteCommand = new DeletePostCommand(post.Id);

                        var createResult = await _mediator.Send(deleteCommand);
                    }
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


        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task<SyncResult> SyncJobTitleAsync()
        {
            var result = new SyncResult();
            await _mapService.FillJobTitleMap();
            var list = await _uow.JobTitleMapRepository.GetAllAsync();
            var existList = await _hrUow.JobTitleRepository.GetAllAsync();

            foreach (var item in list)
            {

                if (item.IrisaJobTitle != null)
                {
                    var existEntity = existList.Where(a => a.Id == item.FkJobTitleId).SingleOrDefault();
                    if (existEntity != null)
                    {
                        if (existEntity.Name.Trim() != item.JobTitle?.Trim())
                        {
                            existEntity.SetName(item.JobTitle);
                            await _hrUow.JobTitleRepository.UpdateAsync(existEntity);
                            //existEntity.AddDomainEvent(new ChangeJobTitleEvent(existEntity.Id));
                            result.UpdatedCount++;
                        }
                    }
                    else
                    {
                        JobTitle model = new JobTitle(item.IrisaJobTitleId.ToString(), item.IrisaJobTitle);
                        await _hrUow.JobTitleRepository.AddAsync(model);
                        item.FkJobTitleId = model.Id;
                        item.JobTitle = model.Name;
                        await _uow.JobTitleMapRepository.UpdateAsync(item);
                        result.AddedCount++;
                    }
                }


                await _hrUow.SaveChangesAsync();
                await _uow.SaveChangesAsync();
            }
            return result;
        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task<SyncResult> SyncJobLevelAsync()
        {
            var result = new SyncResult();
            await _mapService.FillJobLevelMap();
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
                            if (existEntity.Title.Trim() != item.JobLevel?.Trim())
                            {
                                existEntity.SetTitle(item.JobLevel);
                                await _hrUow.JobLevelRepository.UpdateAsync(existEntity);
                                result.UpdatedCount++;
                            }
                        }
                        else
                        {
                            JobLevel model = new JobLevel(item.IrisaJobLevelId.ToString(), item.IrisaJobLevel);
                            await _hrUow.JobLevelRepository.AddAsync(model);
                            item.FkJobLevelId = model.Id;
                            item.JobLevel = model.Title;
                            await _uow.JobLevelMapRepository.UpdateAsync(item);
                            result.AddedCount++;
                        }
                    }
                }

                await _hrUow.SaveChangesAsync();
                await _uow.SaveChangesAsync();

            }
            return result;
        }

        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        public async Task<SyncResult> SyncOrganizationUnitAsync()
        {
            var result = new SyncResult();
            await _mapService.FillOrganizationUnitRootMap();
            await _mapService.FillOrganizationUnitMap();
            var list = await _uow.OrganizationUnitMapRepository.GetAllAsync();
            var existList = await _hrUow.OrganizationUnitRepository.GetAllAsync();
            // roots node
            foreach (var item in list.Where(i => i.IrisaParentId == null))
            {
                if (item.IrisaOrganizationUnit != null)
                {
                    var existEntity = existList.Where(a => a.Id == item.FkOrganizationUnitId).SingleOrDefault();
                    if (existEntity != null)
                    {
                        if (existEntity.Name.Trim() != item.OrganizationUnit?.Trim())
                        {
                            existEntity.SetName(item.OrganizationUnit);
                            await _hrUow.OrganizationUnitRepository.UpdateAsync(existEntity);
                            result.UpdatedCount++;
                        }
                    }
                    else
                    {
                        OrganizationUnit model = new OrganizationUnit(item.IrisaOrganizationUnit, item.IrisaOrganizationUnitId.ToString(), null);
                        await _hrUow.OrganizationUnitRepository.AddAsync(model);
                        item.FkOrganizationUnitId = model.Id;
                        item.OrganizationUnit = model.Name;
                        await _uow.OrganizationUnitMapRepository.UpdateAsync(item);
                        result.AddedCount++;
                    }
                }
            }
            var trmp = list.Where(i => i.IrisaParentId != null).ToList();
            //Child Node
            foreach (var item in list.Where(i => i.IrisaParentId != null))
            {
                if (item.IrisaOrganizationUnit != null)
                {
                    var existEntity = existList.Where(a => a.Id == item.FkOrganizationUnitId).SingleOrDefault();
                    IrisaSyncOrganizationUnitMap parentMap = list.Where(i => i.IrisaOrganizationUnitId == item.IrisaParentId).SingleOrDefault();
                    if (existEntity != null)
                    {
                        if (existEntity.Name?.Trim() != item.OrganizationUnit?.Trim() || existEntity.FkParentId != parentMap?.FkOrganizationUnitId)
                        {
                            existEntity.SetName(item.OrganizationUnit);
                            existEntity.SetParent(parentMap?.FkOrganizationUnitId);
                            await _hrUow.OrganizationUnitRepository.UpdateAsync(existEntity);
                            result.UpdatedCount++;
                        }
                    }
                    else
                    {
                        OrganizationUnit model = new OrganizationUnit(item.IrisaOrganizationUnit, item.IrisaOrganizationUnitId.ToString(), parentMap?.FkOrganizationUnitId);
                        await _hrUow.OrganizationUnitRepository.AddAsync(model);
                        item.FkOrganizationUnitId = model.Id;
                        item.OrganizationUnit = model.Name;
                        await _uow.OrganizationUnitMapRepository.UpdateAsync(item);
                        result.AddedCount++;
                    }
                }
            }
            await _hrUow.SaveChangesAsync();
            await _uow.SaveChangesAsync();

            return result;
        }

        public async Task<SyncResult> SyncAssignmentsAsync()
        {
            var result = new SyncResult();

            try
            {
                // 1. دریافت کارمندان از ویو خارجی (فیلتر شده)
                List<PdsIdeaInformationViw> externalList = (await _irisaRepo.GetAllAsync())
                    .Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null)
                    .ToList();

                if (!externalList.Any())
                {
                    result.Errors.Add("No external employment data found.");
                    return result;
                }

                // 2. دریافت کارمندان موجود در دیتابیس (برای یافتن EmploymentId)
                var existingEmployments = await _hrUow.EmployementInfoViewRepository.GetAllAsync();
                var employmentDict = existingEmployments
                    .ToDictionary(e => e.EmploymentCode, e => e); // کلید: کد پرسنلی

                // 3. دریافت مپ‌های عنوان شغلی (Irisa → داخلی)
                var jobTitleMap = (await _uow.JobTitleMapRepository.GetAllAsync())
                    .Where(j => j.IrisaJobTitleId != null)
                    .ToDictionary(j => j.IrisaJobTitleId, j => j.FkJobTitleId);

                // 4. دریافت تمام پست‌های فعال (برای یافتن پست متناظر با کلید JobTitleId + Code)
                var allPosts = await _hrUow.PostRepository
                    .GetAllAsync(queryOptions: q => q.Where(p => p.IsRemove != true));
                var postDict = allPosts
                    .Where(p => p.FkJobTitleId != Guid.Empty && !string.IsNullOrEmpty(p.Code))
                    .ToDictionary(
                        p => (p.FkJobTitleId, p.Code),
                        p => p
                    );

                // 5. مجموعه کدهای پرسنلی موجود در سیستم خارجی (برای تشخیص کارمندانی که باید انتسابشان خالی شود)
                var externalEmploymentCodes = externalList
                    .Select(e => e.NumPrsnEmply.ToString())
                    .ToHashSet();


                // 7. گروه‌بندی کارمندان خارجی بر اساس عنوان شغلی (CodJobpo)
                var groups = externalList
                    .GroupBy(e => e.CodJobpo)
                    .ToList();

                foreach (var group in groups)
                {
                    var irisJobTitleId = group.Key;
                    if (irisJobTitleId == null) continue;

                    // یافتن JobTitleId داخلی
                    if (!jobTitleMap.TryGetValue(irisJobTitleId, out var jobTitleId))
                    {
                        result.Errors.Add($"JobTitle map not found for Irisa ID: {irisJobTitleId}");
                        continue;
                    }

                    // مرتب‌سازی کارمندان گروه بر اساس کد پرسنلی (برای تطابق با ترتیب ایجاد پست‌ها)
                    var sortedEmployees = group
                        .OrderBy(e => e.NumPrsnEmply)
                        .ToList();

                    int counter = 1;
                    foreach (var ext in sortedEmployees)
                    {
                        try
                        {
                            string employmentCode = ext.NumPrsnEmply.ToString();

                            // بررسی وجود کارمند در سیستم داخلی
                            if (!employmentDict.TryGetValue(employmentCode, out var employment))
                            {
                                result.Errors.Add($"Employment not found in internal system: {employmentCode}");
                                continue;
                            }

                            // ساخت کد پست بر اساس شمارنده (همانند همگام‌سازی پست‌ها)
                            string postCode = counter.ToString();
                            var key = (JobTitleId: (Guid)jobTitleId, Code: postCode);

                            // یافتن پست متناظر
                            if (!postDict.TryGetValue(key, out var post))
                            {
                                result.Errors.Add($"Post not found for JobTitleId: {jobTitleId}, Code: {postCode}");
                                continue;
                            }

                            // انتصاب کارمند به این پست
                            var assignResult = await _postService.AssignToEmploymentAsync(
                                postId: new List<Guid?> { post.Id },
                                employmentId: employment.Id,
                                assigneType: PostAssignmentType.Delegation,
                                EffectiveFrom: Convert.ToDateTime(ext.DatEmpltEmplyEn),
                                EffectiveTo: null
                            );

                            if (assignResult != Guid.Empty)
                            {
                                result.UpdatedCount++; // یا AddedCount، بسته به نیاز
                            }
                            else
                            {
                                // اگر هیچ انتسابی اضافه نشد (مثلاً قبلاً همین انتساب وجود داشت)، باز هم موفق محسوب می‌شود
                                // ولی برای شمارش، می‌توانیم آن را به‌عنوان موفق در نظر بگیریم
                                result.UpdatedCount++;
                            }

                            counter++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error assigning employment {ext.NumPrsnEmply}: {ex.Message}");
                        }
                    }
                }

                // 8. منقضی کردن انتسابات کارمندانی که در سیستم خارجی وجود ندارند
                var employmentsToClear = employmentDict.Keys
                    .Where(code => !externalEmploymentCodes.Contains(code))
                    .ToList();

                foreach (var employmentCode in employmentsToClear)
                {
                    try
                    {
                        var employment = employmentDict[employmentCode];
                        // فراخوانی با لیست خالی → تمام انتسابات فعال منقضی می‌شوند
                        await _postService.AssignToEmploymentAsync(
                            postId: new List<Guid?>(),
                            employmentId: employment.Id,
                            assigneType: null,
                            EffectiveFrom: null,
                            EffectiveTo: null
                        );
                        // شمارش حذف‌ها (اختیاری)
                        result.DeletedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error clearing assignments for employment {employmentCode}: {ex.Message}");
                    }
                }

                // 9. ذخیره‌سازی نهایی تغییرات (اگر متد AssignToEmploymentAsync خودش Save نمی‌کند)
                await _hrUow.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                result.Errors.Add($"SyncAssignments failed: {ex.Message}");
            }

            return result;
        }
    }
}
