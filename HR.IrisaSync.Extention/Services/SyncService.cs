using Azure.Core;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Application.Helper;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Exporter.Excel;
using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using DocumentFormat.OpenXml.Office.CustomUI;
using HR.Application.Commands.Assignment;
using HR.Application.Commands.Employment;
using HR.Application.Commands.JobLevel;
using HR.Application.Commands.JobTitle;
using HR.Application.Commands.OrganizationUnit;
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
using HR.IrisaSync.Extention.Services;
using HR.IrisaSync.Extention.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HR.IrisaSync.Extention.Services
{
    
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
        #region Employment

        private bool HasEmploymentChanged(PdsIdeaInformationViw ext, EmploymentInfoView existing, Dictionary<Guid, List<ContactItemDto>> allContacts)
        {
            #region مقایسه اطلاعات مربوط به شخصیت حقیقی فرد 
            bool hasChange =
                ext.NamFirstEmply != existing.FirstName ||
                ext.NamLastEmply != existing.LastName ||
                ext.DesSexEmply?.Trim() != (existing.Gender == (int)Gender.Male ? "مذکر" : "مونث") ||
                ext.CodNatEmply != existing.NationalCode;

            if (hasChange)
                return true;
            #endregion

            #region مقایسه اطلاعات مربوط به مشخصات کارمندی
            hasChange =
                DateOnly.FromDateTime(Convert.ToDateTime(ext.DatEmpltEmplyEn)) != existing.EmploymentEffectiveFrom ||
                ext.NumPrsnEmply.ToString() != existing.EmploymentCode?.Trim();

            if (hasChange)
                return true;
            #endregion

            #region مقایسه اطلاعات تماس شخصیت حقیقی
            List<ContactItemDto> contacts = new();
            if (existing.FkPartyContactProfileId != null &&
                allContacts.TryGetValue(existing.FkPartyContactProfileId.Value, out var foundContacts))
            {
                contacts = foundContacts;
            }

            string tel = ext.NumTelEmply?.ToString() ?? string.Empty;
            string mobile = ext.NumMobilEmply?.ToString() ?? string.Empty;
            string address = ext.DesAdrEmply ?? string.Empty;

            hasChange = !contacts.Any(c => c.Value == tel) ||
                        !contacts.Any(c => c.Value == address) ||
                        !contacts.Any(c => c.Value == mobile);
            #endregion

            return hasChange;
        }

        private CreateEmploymentCommand CreateCreateCommand(PdsIdeaInformationViw ext)
        {
            return new CreateEmploymentCommand(
                Phone: new List<string> { ext.NumTelEmply?.ToString() ?? string.Empty },
                Address: new List<string> { ext.DesAdrEmply ?? string.Empty },
                Email: null,
                Mobile: new List<string> { ext.NumMobilEmply?.ToString() ?? string.Empty },
                OfficePhone: null,
                OrgEmail: null,
                OrgMobile: null,
                NationalCode: ext.CodNatEmply,
                FirstName: ext.NamFirstEmply,
                LastName: ext.NamLastEmply,
                BirthDate: Convert.ToDateTime(ext.DatBirthEmplyEn),
                BirthPlace: ext.BirthPlace,
                FatherName: ext.NamFathrEmply,
                Gender: ext.DesSexEmply?.Trim() == "مذکر" ? Gender.Male : Gender.Female,
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
        }

        private UpdateEmploymentCommand CreateUpdateCommand(PdsIdeaInformationViw ext, EmploymentInfoView existing)
        {
            return new UpdateEmploymentCommand(
                Id: existing.Id,
                Phone: new List<string> { ext.NumTelEmply?.ToString() ?? string.Empty },
                Address: new List<string> { ext.DesAdrEmply ?? string.Empty },
                Mobile: new List<string> { ext.NumMobilEmply?.ToString() ?? string.Empty },
                nationalCode: ext.CodNatEmply,
                FirstName: ext.NamFirstEmply,
                LastName: ext.NamLastEmply,
                BirthDate: Convert.ToDateTime(ext.DatBirthEmplyEn),
                BirthPlace: ext.BirthPlace,
                FatherName: ext.NamFathrEmply,
                EmploymentCode: ext.NumPrsnEmply.ToString(),
                Gender: ext.DesSexEmply?.Trim() == "مذکر" ? Gender.Male : Gender.Female,
                StartDate: DateOnly.FromDateTime(Convert.ToDateTime(ext.DatEmpltEmplyEn))
            );
        }

        public async Task<BatchResult<SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand>>> SyncEmploymentsPreviewAsync()
        {
            var bundle = new SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand>();
            var errors = new List<string>();

            try
            {
                // ۱. دریافت داده‌های خارجی
                List<PdsIdeaInformationViw> externalEmployments = (await _irisaRepo.GetAllAsync(queryOptions: q => q.Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null))).ToList();

                if (!externalEmployments.Any())
                {
                    bundle.Warnings.Add("هیچ داده‌ای از سیستم مبدأ دریافت نشد. عملیات همگام‌سازی متوقف شد.");
                    return new BatchResult<SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand>>(true, Data: bundle);
                }

                // ۲. دریافت داده‌های موجود
                IEnumerable<EmploymentInfoView> existingEmployments = await _hrUow.EmployementInfoViewRepository.GetAllAsync();
                var existingDict = existingEmployments.ToDictionary(e => e.EmploymentCode, e => e);

                var contactProfileIds = existingEmployments
                    .Where(e => e.FkPartyContactProfileId != null)
                    .Select(e => e.FkPartyContactProfileId.Value)
                    .Distinct()
                    .ToList();

                var allContacts = (await _contactService.GetContactsByProfilesIdsAsync(contactProfileIds))
                    .GroupBy(c => c.ProfileId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // ۳. پردازش هر رکورد خارجی
                foreach (var ext in externalEmployments)
                {
                    string personalCode = ext.NumPrsnEmply.ToString();

                    if (existingDict.TryGetValue(personalCode, out var existing))
                    {
                        // بررسی تغییرات
                        if (HasEmploymentChanged(ext, existing, allContacts))
                        {
                            var updateCommand = CreateUpdateCommand(ext, existing);
                            bundle.UpdateCommands.Add(new SyncPreviewItem<UpdateEmploymentCommand>
                            {
                                Summary = $"ویرایش اطلاعات کارمند '{existing.FirstName} {existing.LastName}' (کد پرسنلی: {personalCode})",
                                Command = updateCommand
                            });
                        }

                        existingDict.Remove(personalCode);
                    }
                    else
                    {
                        // رکورد جدید
                        var createCommand = CreateCreateCommand(ext);
                        bundle.AddCommands.Add(new SyncPreviewItem<CreateEmploymentCommand>
                        {
                            Summary = $"افزودن کارمند جدید '{ext.NamFirstEmply} {ext.NamLastEmply}' (کد پرسنلی: {personalCode})",
                            Command = createCommand
                        });
                    }
                }

                // ۴. رکوردهای باقی‌مانده => حذف
                foreach (var emp in existingDict.Values)
                {
                    var deleteCommand = new DeleteEmploymentCommand(emp.Id);
                    bundle.DeleteCommands.Add(new SyncPreviewItem<DeleteEmploymentCommand>
                    {
                        Summary = $"حذف کارمند '{emp.FirstName} {emp.LastName}' (کد پرسنلی: {emp.EmploymentCode})",
                        Command = deleteCommand
                    });
                }

                return new BatchResult<SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand>>(true, Data: bundle);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand>>.Fail($"{IconInTextHelper.IconError} خطا در پیش‌نمایش همگام‌سازی کارمندان: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> ApplyEmploymentsAsync(SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand> selectedBundle)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();
            var addedCount = 0;
            var updatedCount = 0;
            var deletedCount = 0;

            try
            {
                // ۱. اجرای دستورات افزودن
                foreach (var item in selectedBundle.AddCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۲. اجرای دستورات بروزرسانی (دسته‌جمعی)
                if (selectedBundle.UpdateCommands.Any())
                {
                    try
                    {
                        var updateCmdList = selectedBundle.UpdateCommands.Select(x => x.Command).ToList();
                        var batchUpdateCommand = new BatchUpdateEmploymentsCommand(updateCmdList);
                        var batchResult = await _mediator.Send(batchUpdateCommand);

                        successMessages.AddRange(batchResult.SuccessMessages);
                        errors.AddRange(batchResult.Errors);
                        updatedCount = batchResult.SuccessMessages.Count;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در بروزرسانی دسته‌جمعی کارمندان: {ex.Message}");
                    }
                }

                // ۳. اجرای دستورات حذف
                foreach (var item in selectedBundle.DeleteCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        deletedCount++;
                        successMessages.Add($"{IconInTextHelper.IconDelete} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                var syncResult = new SyncResult { AddedCount = addedCount, UpdatedCount = updatedCount, DeletedCount = deletedCount };
                return new BatchResult<SyncResult>(!errors.Any(), successMessages, errors, syncResult);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncResult>.Fail($"{IconInTextHelper.IconError} خطا در اعمال همگام‌سازی کارمندان: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> SyncEmploymentsAsync()
        {
            var previewResult = await SyncEmploymentsPreviewAsync();

            if (!previewResult.Succeeded || previewResult.Data == null)
            {
                return BatchResult<SyncResult>.Fail(previewResult.Errors);
            }

            return await ApplyEmploymentsAsync(previewResult.Data);
        }

        #endregion
        #region Post

        public async Task<BatchResult<SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand>>> SyncPostPreviewAsync()
        {
            var bundle = new SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand>();

            try
            {
                // ۱. دریافت داده‌های خارجی و گروه‌بندی
                var irisaGroups = (await _irisaRepo.GetAllAsync(queryOptions: q => q.Where(e => e.CodEmtyp == true && e.CodJobpo != null)))
                    .GroupBy(a => a.CodJobpo)
                    .ToList();

                if (!irisaGroups.Any())
                {
                    bundle.Warnings.Add("هیچ داده‌ای از پست‌ها در سیستم خارجی یافت نشد.");
                    return new BatchResult<SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand>>(true, Data: bundle);
                }

                // ۲. دریافت مپ‌ها
                var jobTitleMap = (await _uow.JobTitleMapRepository.GetAllAsync())
                    .Where(a => a.IrisaJobTitleId != null)
                    .ToDictionary(j => j.IrisaJobTitleId, j => j.FkJobTitleId);

                var jobLevelMap = (await _uow.JobLevelMapRepository.GetAllAsync())
                    .Where(a => a.IrisaJobLevelId != null)
                    .ToDictionary(j => j.IrisaJobLevelId, j => j.FkJobLevelId);

                var organUnitMap = (await _uow.OrganizationUnitMapRepository.GetAllAsync())
                    .Where(a => a.IrisaOrganizationUnitId != null)
                    .ToDictionary(j => j.IrisaOrganizationUnitId, j => j.FkOrganizationUnitId);

                // ۳. دریافت پست‌های موجود
                var existingPosts = await _hrUow.PostRepository
                    .GetAllAsync(queryOptions: q => q.Where(a => a.IsRemove != true).Include(b => b.JobTitle));

                var existingDict = existingPosts
                    .Where(p => p.FkJobTitleId != Guid.Empty && !string.IsNullOrEmpty(p.Code))
                    .ToDictionary(
                        p => (p.FkJobTitleId, p.Code),
                        p => p
                    );

                // ۴. پردازش هر گروه
                foreach (var group in irisaGroups)
                {
                    if (!jobTitleMap.TryGetValue(group.Key, out var jobTitleId) || jobTitleId == null)
                    {
                        bundle.Warnings.Add($"عنوان شغلی متناظر با شناسه سیستم خارجی '{group.Key}' پیدا نشد.");
                        continue;
                    }

                    int counter = 0;
                    var sortedGroup = group.OrderBy(e => e.NumPrsnEmply).ToList();

                    foreach (var item in sortedGroup)
                    {
                        counter++;
                        string code = counter.ToString();

                        jobLevelMap.TryGetValue(item.CodPosit, out var jobLevelId);
                        organUnitMap.TryGetValue(item.CodBusun, out var orgUnitId);

                        var key = (JobTitleId: (Guid)jobTitleId, Code: code);

                        if (existingDict.TryGetValue(key, out var existingPost))
                        {
                            // بررسی تغییرات
                            bool hasChange = existingPost.FkOrganizationUnitId != orgUnitId ||
                                            existingPost.FkJobLevelId != jobLevelId;

                            if (hasChange)
                            {
                                var updateCommand = new UpdatePostCommand(
                                    Id: existingPost.Id,
                                    OrganizationUnitId: orgUnitId,
                                    JobLevelId: jobLevelId
                                );

                                bundle.UpdateCommands.Add(new SyncPreviewItem<UpdatePostCommand>
                                {
                                    Summary = $"ویرایش پست '{existingPost.JobTitle?.Name ?? code}' (کد: {code})",
                                    Command = updateCommand
                                });
                            }

                            existingDict.Remove(key);
                        }
                        else
                        {
                            // پست جدید
                            if (orgUnitId == null)
                            {
                                bundle.Warnings.Add($"واحد سازمانی متناظر برای پست کد '{code}' عنوان شغلی '{item.DesJobpo}' یافت نشد.");
                                continue;
                            }

                            var createCommand = new CreatePostCommand(
                                code,
                                (Guid)orgUnitId,
                                (Guid)jobTitleId,
                                jobLevelId,
                                null, null, null, true,
                                null, null, null, null, null, null
                            );

                            bundle.AddCommands.Add(new SyncPreviewItem<CreatePostCommand>
                            {
                                Summary = $"افزودن پست جدید با کد '{code}' برای عنوان شغلی '{item.DesJobpo ?? "نامشخص"}'",
                                Command = createCommand
                            });
                        }
                    }
                }

                // ۵. پست‌های باقی‌مانده => باید حذف شوند
                foreach (var post in existingDict.Values)
                {
                    var deleteCommand = new DeletePostCommand(post.Id);

                    bundle.DeleteCommands.Add(new SyncPreviewItem<DeletePostCommand>
                    {
                        Summary = $"حذف پست '{post.JobTitle?.Name ?? post.Code}' (کد: {post.Code})",
                        Command = deleteCommand
                    });
                }

                return new BatchResult<SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand>>(true, Data: bundle);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand>>.Fail($"{IconInTextHelper.IconError} خطا در پیش‌نمایش پست‌ها: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> ApplyPostAsync(SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand> selectedBundle)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();
            var addedCount = 0;
            var updatedCount = 0;
            var deletedCount = 0;

            try
            {
                // ۱. اجرای دستورات افزودن
                foreach (var item in selectedBundle.AddCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۲. اجرای دستورات بروزرسانی (دسته‌جمعی)
                if (selectedBundle.UpdateCommands.Any())
                {
                    try
                    {
                        var updateCmdList = selectedBundle.UpdateCommands.Select(x => new UpdatePostCommand(
                            x.Command.Id, null, x.Command.OrganizationUnitId, x.Command.JobTitleId, x.Command.JobLevelId,
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
                        )).ToList();

                        var batchUpdateCommand = new BatchUpdatePostsCommand(updateCmdList);
                        var batchResult = await _mediator.Send(batchUpdateCommand);

                        successMessages.AddRange(batchResult.SuccessMessages);
                        errors.AddRange(batchResult.Errors);
                        updatedCount = batchResult.SuccessMessages.Count;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در بروزرسانی دسته‌جمعی پست‌ها: {ex.Message}");
                    }
                }

                // ۳. اجرای دستورات حذف
                foreach (var item in selectedBundle.DeleteCommands)
                {
                    try
                    {
                        var result = await _mediator.Send(item.Command);
                        if (result.Succeeded)
                        {
                            deletedCount++;
                            successMessages.Add($"{IconInTextHelper.IconDelete} {item.Summary} با موفقیت انجام شد.");
                        }
                        else
                        {
                            errors.Add($"{IconInTextHelper.IconError} {item.Summary} ناموفق بود.");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                await _hrUow.SaveChangesAsync();

                var syncResult = new SyncResult { AddedCount = addedCount, UpdatedCount = updatedCount, DeletedCount = deletedCount };
                return new BatchResult<SyncResult>(!errors.Any(), successMessages, errors, syncResult);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncResult>.Fail($"{IconInTextHelper.IconError} خطا در اعمال همگام‌سازی پست‌ها: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> SyncPostAsync()
        {
            var previewResult = await SyncPostPreviewAsync();

            if (!previewResult.Succeeded || previewResult.Data == null)
            {
                return BatchResult<SyncResult>.Fail(previewResult.Errors);
            }

            var applyResult = await ApplyPostAsync(previewResult.Data);

            if (previewResult.Data.Warnings.Any())
            {
                applyResult.Errors.AddRange(previewResult.Data.Warnings);
            }

            return applyResult;
        }

        #endregion
        #region Assignment

        public async Task<BatchResult<SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand>>> SyncAssignmentsPreviewAsync()
        {
            var bundle = new SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand>();

            try
            {
                // ۱. دریافت داده‌های خارجی
                var externalList = (await _irisaRepo.GetAllAsync(queryOptions: q => q.Where(e => e.CodEmtyp == true && e.NumPrsnEmply != null)))
                    .ToList();

                if (!externalList.Any())
                {
                    bundle.Warnings.Add("هیچ داده‌ای از سیستم خارجی یافت نشد.");
                    return new BatchResult<SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand>>(true, Data: bundle);
                }

                // ۲. دریافت اطلاعات پایه
                var existingEmployments = await _hrUow.EmployementInfoViewRepository.GetAllAsync();
                var employmentDict = existingEmployments.ToDictionary(e => e.EmploymentCode, e => e);

                var jobTitleMap = (await _uow.JobTitleMapRepository.GetAllAsync())
                    .Where(j => j.IrisaJobTitleId != null)
                    .ToDictionary(j => j.IrisaJobTitleId, j => j.FkJobTitleId);

                var allPosts = await _hrUow.PostRepository
                    .GetAllAsync(queryOptions: q => q.Where(p => p.IsRemove != true).Include(a => a.JobTitle));
                var postDict = allPosts
                    .Where(p => p.FkJobTitleId != Guid.Empty && !string.IsNullOrEmpty(p.Code))
                    .ToDictionary(p => (p.FkJobTitleId, p.Code), p => p);

                // ۳. دریافت انتصابات فعال
                var currentAssignments = await _hrUow.AssignmentRepository
                    .GetAllAsync(q => q.Where(a => a.IsCurrent == true));

                var employmentAssignments = currentAssignments
                    .GroupBy(a => a.FkEmploymentId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // ۴. بررسی گروه‌ها و ایجاد دستورات Add / Update
                var groups = externalList.GroupBy(e => e.CodJobpo).ToList();

                foreach (var group in groups)
                {
                    var irisJobTitleId = group.Key;
                    if (irisJobTitleId == null) continue;

                    if (!jobTitleMap.TryGetValue(irisJobTitleId, out var jobTitleId))
                    {
                        bundle.Warnings.Add($"عنوان شغلی متناظر یافت نشد: {irisJobTitleId}");
                        continue;
                    }

                    var sortedEmployees = group.OrderBy(e => e.NumPrsnEmply).ToList();
                    int counter = 1;

                    foreach (var ext in sortedEmployees)
                    {
                        try
                        {
                            string employmentCode = ext.NumPrsnEmply.ToString();
                            if (!employmentDict.TryGetValue(employmentCode, out var employment))
                            {
                                bundle.Warnings.Add($"کارمند با کد پرسنلی '{employmentCode}' در سیستم یافت نشد.");
                                continue;
                            }

                            string postCode = counter.ToString();
                            var key = (JobTitleId: (Guid)jobTitleId, Code: postCode);

                            if (!postDict.TryGetValue(key, out var post))
                            {
                                bundle.Warnings.Add($"پست با عنوان '{ext.DesJobpo}' و کد '{postCode}' یافت نشد.");
                                continue;
                            }

                            DateTime effectiveFrom = Convert.ToDateTime(ext.DatEmpltEmplyEn);
                            var activeAssignments = employmentAssignments.GetValueOrDefault(employment.Id, new List<Assignment>());

                            // بررسی وجود انتصاب دقیقا مشابه (پست و تاریخ شروع یکسان)
                            bool isExactMatch = activeAssignments.Any(a =>
                                a.FkPostId == post.Id &&
                                a.EffectiveFrom?.Date == effectiveFrom.Date);

                            if (!isExactMatch)
                            {
                                bool hasActiveAssignments = activeAssignments.Any();

                                if (hasActiveAssignments)
                                {
                                    var updateCmd = new UpdateAssignmentCommand(
                                        employmentId: employment.Id,
                                        postIds: new List<Guid?> { post.Id },
                                        assigneType: PostAssignmentType.Delegation,
                                        effectiveFrom: effectiveFrom,
                                        effectiveTo: null
                                    );

                                    bundle.UpdateCommands.Add(new SyncPreviewItem<UpdateAssignmentCommand>
                                    {
                                        Summary = $"بروزرسانی انتصاب کارمند '{employmentCode}' به پست '{post.JobTitle?.Name}' (کد {post.Code})",
                                        Command = updateCmd
                                    });
                                }
                                else
                                {
                                    var createCmd = new CreateAssignmentCommand(
                                        employmentId: employment.Id,
                                        postIds: new List<Guid?> { post.Id },
                                        assigneType: PostAssignmentType.Delegation,
                                        effectiveFrom: effectiveFrom,
                                        effectiveTo: null
                                    );

                                    bundle.AddCommands.Add(new SyncPreviewItem<CreateAssignmentCommand>
                                    {
                                        Summary = $"افزودن انتصاب کارمند '{employmentCode}' به پست '{post.JobTitle?.Name}' (کد {post.Code})",
                                        Command = createCmd
                                    });
                                }
                            }

                            counter++;
                        }
                        catch (Exception ex)
                        {
                            bundle.Warnings.Add($"خطا در پردازش پیش‌نمایش انتصاب برای کد پرسنلی '{ext.NumPrsnEmply}': {ex.Message}");
                        }
                    }
                }

                // ۵. کارمندانی که در سیستم خارجی نیستند => Delete
                var externalCodes = externalList.Select(e => e.NumPrsnEmply.ToString()).ToHashSet();
                var employmentsToClear = employmentDict.Keys
                    .Where(code => !externalCodes.Contains(code))
                    .ToList();

                foreach (var employmentCode in employmentsToClear)
                {
                    var employment = employmentDict[employmentCode];
                    if (employmentAssignments.TryGetValue(employment.Id, out var activeList) && activeList.Any())
                    {
                        var deleteCmd = new DeleteAssignmentCommand(employment.Id);

                        bundle.DeleteCommands.Add(new SyncPreviewItem<DeleteAssignmentCommand>
                        {
                            Summary = $"حذف تمام انتصابات کارمند '{employmentCode}' (عدم وجود در سیستم خارجی)",
                            Command = deleteCmd
                        });
                    }
                }

                return new BatchResult<SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand>>(true, Data: bundle);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand>>.Fail($"{IconInTextHelper.IconError} خطا در پیش‌نمایش انتصابات: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> ApplyAssignmentsAsync(SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand> selectedBundle)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();
            int addedCount = 0, updatedCount = 0, deletedCount = 0;

            try
            {
                // ۱. اجرای دستورات ایجاد انتصاب
                foreach (var item in selectedBundle.AddCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۲. اجرای دستورات بروزرسانی انتصاب
                foreach (var item in selectedBundle.UpdateCommands)
                {
                    try
                    {
                        var response =  await _mediator.Send(item.Command);
                        if(response.Data == true)
                            updatedCount++;
                        successMessages.Add($"{IconInTextHelper.IconUpdate} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۳. اجرای دستورات حذف انتصاب
                foreach (var item in selectedBundle.DeleteCommands)
                {
                    try
                    {
                       var response = await _mediator.Send(item.Command);
                        if(response.Data == true)
                            deletedCount++;
                        successMessages.Add($"{IconInTextHelper.IconDelete} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                var syncResult = new SyncResult
                {
                    AddedCount = addedCount,
                    UpdatedCount = updatedCount,
                    DeletedCount = deletedCount
                };

                return new BatchResult<SyncResult>(!errors.Any(), successMessages, errors, syncResult);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncResult>.Fail($"{IconInTextHelper.IconError} خطا در اعمال همگام‌سازی انتصابات: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> SyncAssignmentsAsync()
        {
            var previewResult = await SyncAssignmentsPreviewAsync();

            if (!previewResult.Succeeded || previewResult.Data == null)
            {
                return BatchResult<SyncResult>.Fail(previewResult.Errors);
            }

            var applyResult = await ApplyAssignmentsAsync(previewResult.Data);

            if (previewResult.Data.Warnings.Any())
            {
                applyResult.Errors.AddRange(previewResult.Data.Warnings);
            }

            return applyResult;
        }

        #endregion
        /// <summary>
        /// پر کردن جدول اصلی با داده های موجود در جدول مپ
        /// </summary>
        /// <returns></returns>
        /// 
        #region JobTitle

        public async Task<BatchResult<SyncCommandBundle<CreateJobTitleCommand, UpdateJobTitleCommand, DeleteJobTitleCommand>>> SyncJobTitlePreviewAsync()
        {
            var bundle = new SyncCommandBundle<CreateJobTitleCommand, UpdateJobTitleCommand, DeleteJobTitleCommand>();

            try
            {
                await _mapService.FillJobTitleMap();
                var mapList = await _uow.JobTitleMapRepository.GetAllAsync();
                var existList = await _hrUow.JobTitleRepository.GetAllAsync();
                var existDict = existList.ToDictionary(a => a.Id);

                foreach (var item in mapList.Where(i => i.IrisaJobTitle != null))
                {
                    if (item.FkJobTitleId.HasValue && existDict.TryGetValue(item.FkJobTitleId.Value, out var existEntity))
                    {
                        if (existEntity.Name?.Trim() != item.JobTitle?.Trim())
                        {
                            var updateCmd = new UpdateJobTitleCommand(existEntity.Id, Optional<string>.Undefined, item.JobTitle?.Trim(), Optional<bool>.Undefined);

                            bundle.UpdateCommands.Add(new SyncPreviewItem<UpdateJobTitleCommand>
                            {
                                Summary = $"تغییر عنوان شغلی از '{existEntity.Name}' به '{item.JobTitle}'",
                                Command = updateCmd
                            });
                        }
                    }
                    else
                    {
                        var createCmd = new CreateJobTitleCommand(item.IrisaJobTitleId.ToString(), item.IrisaJobTitle);

                        bundle.AddCommands.Add(new SyncPreviewItem<CreateJobTitleCommand>
                        {
                            Summary = $"افزودن عنوان شغلی جدید '{item.IrisaJobTitle}'",
                            Command = createCmd
                        });
                    }
                }

                return new BatchResult<SyncCommandBundle<CreateJobTitleCommand, UpdateJobTitleCommand, DeleteJobTitleCommand>>(true, Data: bundle);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncCommandBundle<CreateJobTitleCommand, UpdateJobTitleCommand, DeleteJobTitleCommand>>.Fail($"{IconInTextHelper.IconError} خطا در پیش‌نمایش عناوین شغلی: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> ApplyJobTitleAsync(SyncCommandBundle<CreateJobTitleCommand, UpdateJobTitleCommand, DeleteJobTitleCommand> selectedBundle)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();
            int addedCount = 0, updatedCount = 0, deletedCount = 0;

            try
            {
                // ۱. اجرای دستورات ایجاد
                foreach (var item in selectedBundle.AddCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۲. اجرای دستورات بروزرسانی
                foreach (var item in selectedBundle.UpdateCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        updatedCount++;
                        successMessages.Add($"{IconInTextHelper.IconUpdate} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۳. اجرای دستورات حذف
                foreach (var item in selectedBundle.DeleteCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        deletedCount++;
                        successMessages.Add($"{IconInTextHelper.IconDelete} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                var syncResult = new SyncResult { AddedCount = addedCount, UpdatedCount = updatedCount, DeletedCount = deletedCount };
                return new BatchResult<SyncResult>(!errors.Any(), successMessages, errors, syncResult);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncResult>.Fail($"{IconInTextHelper.IconError} خطا در اعمال همگام‌سازی عناوین شغلی: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> SyncJobTitleAsync()
        {
            var previewResult = await SyncJobTitlePreviewAsync();

            if (!previewResult.Succeeded || previewResult.Data == null)
            {
                return BatchResult<SyncResult>.Fail(previewResult.Errors);
            }

            var applyResult = await ApplyJobTitleAsync(previewResult.Data);

            if (previewResult.Data.Warnings.Any())
            {
                applyResult.Errors.AddRange(previewResult.Data.Warnings);
            }

            return applyResult;
        }

        #endregion

        #region JobLevel

        public async Task<BatchResult<SyncCommandBundle<CreateJobLevelCommand, UpdateJobLevelCommand, DeleteJobLevelCommand>>> SyncJobLevelPreviewAsync()
        {
            var bundle = new SyncCommandBundle<CreateJobLevelCommand, UpdateJobLevelCommand, DeleteJobLevelCommand>();

            try
            {
                await _mapService.FillJobLevelMap();
                var mapList = await _uow.JobLevelMapRepository.GetAllAsync();
                var existList = await _hrUow.JobLevelRepository.GetAllAsync();
                var existDict = existList.ToDictionary(a => a.Id);

                foreach (var item in mapList.Where(i => i.IrisaJobLevel != null))
                {
                    if (item.FkJobLevelId.HasValue && existDict.TryGetValue(item.FkJobLevelId.Value, out var existEntity))
                    {
                        if (existEntity.Title?.Trim() != item.JobLevel?.Trim())
                        {
                            var updateCmd = new UpdateJobLevelCommand(existEntity.Id, Optional<string>.Undefined, item.JobLevel);

                            bundle.UpdateCommands.Add(new SyncPreviewItem<UpdateJobLevelCommand>
                            {
                                Summary = $"تغییر سطح شغلی از '{existEntity.Title}' به '{item.JobLevel}'",
                                Command = updateCmd
                            });
                        }
                    }
                    else
                    {
                        var createCmd = new CreateJobLevelCommand(item.IrisaJobLevelId.ToString(), item.IrisaJobLevel);

                        bundle.AddCommands.Add(new SyncPreviewItem<CreateJobLevelCommand>
                        {
                            Summary = $"افزودن سطح شغلی جدید '{item.IrisaJobLevel}'",
                            Command = createCmd
                        });
                    }
                }

                return new BatchResult<SyncCommandBundle<CreateJobLevelCommand, UpdateJobLevelCommand, DeleteJobLevelCommand>>(true, Data: bundle);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncCommandBundle<CreateJobLevelCommand, UpdateJobLevelCommand, DeleteJobLevelCommand>>.Fail($"{IconInTextHelper.IconError} خطا در پیش‌نمایش سطوح شغلی: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> ApplyJobLevelAsync(SyncCommandBundle<CreateJobLevelCommand, UpdateJobLevelCommand, DeleteJobLevelCommand> selectedBundle)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();
            int addedCount = 0, updatedCount = 0, deletedCount = 0;

            try
            {
                // ۱. اجرای دستورات ایجاد
                foreach (var item in selectedBundle.AddCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۲. اجرای دستورات بروزرسانی
                foreach (var item in selectedBundle.UpdateCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        updatedCount++;
                        successMessages.Add($"{IconInTextHelper.IconUpdate} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۳. اجرای دستورات حذف
                foreach (var item in selectedBundle.DeleteCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        deletedCount++;
                        successMessages.Add($"{IconInTextHelper.IconDelete} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                var syncResult = new SyncResult { AddedCount = addedCount, UpdatedCount = updatedCount, DeletedCount = deletedCount };
                return new BatchResult<SyncResult>(!errors.Any(), successMessages, errors, syncResult);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncResult>.Fail($"{IconInTextHelper.IconError} خطا در اعمال همگام‌سازی سطوح شغلی: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> SyncJobLevelAsync()
        {
            var previewResult = await SyncJobLevelPreviewAsync();

            if (!previewResult.Succeeded || previewResult.Data == null)
            {
                return BatchResult<SyncResult>.Fail(previewResult.Errors);
            }

            var applyResult = await ApplyJobLevelAsync(previewResult.Data);

            if (previewResult.Data.Warnings.Any())
            {
                applyResult.Errors.AddRange(previewResult.Data.Warnings);
            }

            return applyResult;
        }

        #endregion

        #region Organization Unit

        public async Task<BatchResult<SyncCommandBundle<CreateOrganizationUnitCommand, UpdateOrganizationUnitCommand, DeleteOrganizationUnitCommand>>> SyncOrganizationUnitPreviewAsync()
        {
            var bundle = new SyncCommandBundle<CreateOrganizationUnitCommand, UpdateOrganizationUnitCommand, DeleteOrganizationUnitCommand>();

            try
            {
                await _mapService.FillOrganizationUnitRootMap();
                await _mapService.FillOrganizationUnitMap();

                var list = await _uow.OrganizationUnitMapRepository.GetAllAsync();
                var existList = await _hrUow.OrganizationUnitRepository.GetAllAsync();
                var existDict = existList.ToDictionary(a => a.Id);
                var mapDictByIrisaId = list.ToDictionary(i => i.IrisaOrganizationUnitId);

                // ۱. بررسی ریشه‌ها (Roots)
                foreach (var item in list.Where(i => i.IrisaParentId == null && i.IrisaOrganizationUnit != null))
                {
                    if (item.FkOrganizationUnitId.HasValue && existDict.TryGetValue(item.FkOrganizationUnitId.Value, out var existEntity))
                    {
                        if (existEntity.Name?.Trim() != item.OrganizationUnit?.Trim())
                        {
                            var updateCmd = new UpdateOrganizationUnitCommand(
                                existEntity.Id,
                                Optional<string>.Undefined,
                                item.OrganizationUnit,
                                null
                            );

                            bundle.UpdateCommands.Add(new SyncPreviewItem<UpdateOrganizationUnitCommand>
                            {
                                Summary = $"تغییر نام واحد سازمانی ریشه از '{existEntity.Name}' به '{item.OrganizationUnit}'",
                                Command = updateCmd
                            });
                        }
                    }
                    else
                    {
                        var createCmd = new CreateOrganizationUnitCommand(
                            item.IrisaOrganizationUnitId.ToString(),
                            item.IrisaOrganizationUnit,
                            null
                        );

                        bundle.AddCommands.Add(new SyncPreviewItem<CreateOrganizationUnitCommand>
                        {
                            Summary = $"افزودن واحد سازمانی ریشه جدید '{item.IrisaOrganizationUnit}'",
                            Command = createCmd
                        });
                    }
                }

                // ۲. بررسی فرزندان (Children)
                foreach (var item in list.Where(i => i.IrisaParentId != null && i.IrisaOrganizationUnit != null))
                {
                    mapDictByIrisaId.TryGetValue(item.IrisaParentId.Value, out var parentMap);

                    if (item.FkOrganizationUnitId.HasValue && existDict.TryGetValue(item.FkOrganizationUnitId.Value, out var existEntity))
                    {
                        bool nameChanged = existEntity.Name?.Trim() != item.OrganizationUnit?.Trim();
                        bool parentChanged = existEntity.ParentId != parentMap?.FkOrganizationUnitId;

                        if (nameChanged || parentChanged)
                        {
                            var updateCmd = new UpdateOrganizationUnitCommand(
                                existEntity.Id,
                                Optional<string>.Undefined,
                                item.OrganizationUnit,
                                parentMap?.FkOrganizationUnitId
                            );

                            bundle.UpdateCommands.Add(new SyncPreviewItem<UpdateOrganizationUnitCommand>
                            {
                                Summary = $"بروزرسانی واحد سازمانی '{existEntity.Name}'",
                                Command = updateCmd
                            });
                        }
                    }
                    else
                    {
                        var createCmd = new CreateOrganizationUnitCommand(
                            item.IrisaOrganizationUnitId.ToString(),
                            item.IrisaOrganizationUnit,
                            parentMap?.FkOrganizationUnitId
                        );

                        bundle.AddCommands.Add(new SyncPreviewItem<CreateOrganizationUnitCommand>
                        {
                            Summary = $"افزودن واحد سازمانی جدید '{item.IrisaOrganizationUnit}'",
                            Command = createCmd
                        });
                    }
                }

                return new BatchResult<SyncCommandBundle<CreateOrganizationUnitCommand, UpdateOrganizationUnitCommand, DeleteOrganizationUnitCommand>>(true, Data: bundle);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncCommandBundle<CreateOrganizationUnitCommand, UpdateOrganizationUnitCommand, DeleteOrganizationUnitCommand>>.Fail($"{IconInTextHelper.IconError} خطا در پیش‌نمایش واحدهای سازمانی: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> ApplyOrganizationUnitAsync(SyncCommandBundle<CreateOrganizationUnitCommand, UpdateOrganizationUnitCommand, DeleteOrganizationUnitCommand> selectedBundle)
        {
            var successMessages = new List<string>();
            var errors = new List<string>();
            int addedCount = 0, updatedCount = 0, deletedCount = 0;

            try
            {
                // ۱. ابتدا ایجاد گره‌های ریشه (ParentId == null)
                var rootCreateCmds = selectedBundle.AddCommands.Where(c => c.Command.ParentId == null).ToList();
                foreach (var item in rootCreateCmds)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۲. ایجاد گره‌های فرزند (ParentId != null)
                var childCreateCmds = selectedBundle.AddCommands.Where(c => c.Command.ParentId != null).ToList();
                foreach (var item in childCreateCmds)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        addedCount++;
                        successMessages.Add($"{IconInTextHelper.IconAdd} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۳. بروزرسانی‌ها
                foreach (var item in selectedBundle.UpdateCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        updatedCount++;
                        successMessages.Add($"{IconInTextHelper.IconUpdate} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                // ۴. حذف‌ها
                foreach (var item in selectedBundle.DeleteCommands)
                {
                    try
                    {
                        await _mediator.Send(item.Command);
                        deletedCount++;
                        successMessages.Add($"{IconInTextHelper.IconDelete} {item.Summary} با موفقیت انجام شد.");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{IconInTextHelper.IconError} خطا در {item.Summary}: {ex.Message}");
                    }
                }

                var syncResult = new SyncResult { AddedCount = addedCount, UpdatedCount = updatedCount, DeletedCount = deletedCount };
                return new BatchResult<SyncResult>(!errors.Any(), successMessages, errors, syncResult);
            }
            catch (Exception ex)
            {
                return BatchResult<SyncResult>.Fail($"{IconInTextHelper.IconError} خطا در اعمال همگام‌سازی واحدهای سازمانی: {ex.Message}");
            }
        }

        public async Task<BatchResult<SyncResult>> SyncOrganizationUnitAsync()
        {
            var previewResult = await SyncOrganizationUnitPreviewAsync();

            if (!previewResult.Succeeded || previewResult.Data == null)
            {
                return BatchResult<SyncResult>.Fail(previewResult.Errors);
            }

            var applyResult = await ApplyOrganizationUnitAsync(previewResult.Data);

            if (previewResult.Data.Warnings.Any())
            {
                applyResult.Errors.AddRange(previewResult.Data.Warnings);
            }

            return applyResult;
        }

        #endregion
    }
}
