using Core.Application.Helper;
using Core.Shared.Results;
using HR.Application.Commands.Assignment;
using HR.Application.Commands.Employment;
using HR.Application.Commands.OrgChart;
using HR.IrisaSync.Extention.Commands.JobLevel;
using HR.IrisaSync.Extention.Commands.JobTitle;
using HR.IrisaSync.Extention.Commands.OrganizationUnit;
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Interface
{
    /// <summary>
    /// پوشش‌دهنده هر دستور همراه با متن توصیفی جهت نمایش در فرانت‌اند
    /// </summary>
    public class SyncPreviewItem<TCommand>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// متن خلاصه و شفاف ساخته‌شده در بک‌اند (مثلاً: "ویرایش عنوان شغلی از 'کارشناس' به 'مدیر'")
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// دستور اصلی CQRS جهت ارسال به MediatR در صورت تایید کاربر
        /// </summary>
        public TCommand Command { get; set; } = default!;
    }

    /// <summary>
    /// باندل یکپارچه پیش‌نمایش تغییرات برای تمام موجودیت‌ها
    /// </summary>
    public class SyncCommandBundle<TCreate, TUpdate, TDelete>
    {
        public List<SyncPreviewItem<TCreate>> AddCommands { get; set; } = new();
        public List<SyncPreviewItem<TUpdate>> UpdateCommands { get; set; } = new();
        public List<SyncPreviewItem<TDelete>> DeleteCommands { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
    public class SyncResult
    {
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int DeletedCount { get; set; }
        public override string ToString()
            => $"Added: {AddedCount}, Updated: {UpdatedCount}, Deleted: {DeletedCount}";
    }

    public interface ISyncService
    {
        #region Employment
        Task<BatchResult<SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand>>> SyncEmploymentsPreviewAsync();
        Task<BatchResult<SyncResult>> ApplyEmploymentsAsync(SyncCommandBundle<CreateEmploymentCommand, UpdateEmploymentCommand, DeleteEmploymentCommand> selectedBundle);
        Task<BatchResult<SyncResult>> SyncEmploymentsAsync();
        #endregion

        #region JobTitle
        Task<BatchResult<SyncCommandBundle<CreateJobTitleIrisaSyncCommand, UpdateJobTitleIrisaSyncCommand, DeleteJobTitleIrisaSyncCommand>>> SyncJobTitlePreviewAsync();
        Task<BatchResult<SyncResult>> ApplyJobTitleAsync(SyncCommandBundle<CreateJobTitleIrisaSyncCommand, UpdateJobTitleIrisaSyncCommand, DeleteJobTitleIrisaSyncCommand> selectedBundle);
        Task<BatchResult<SyncResult>> SyncJobTitleAsync();
        #endregion

        #region JobLevel
        Task<BatchResult<SyncCommandBundle<CreateJobLevelIrisaSyncCommand, UpdateJobLevelIrisaSyncCommand, DeleteJobLevelIrisaSyncCommand>>> SyncJobLevelPreviewAsync();
        Task<BatchResult<SyncResult>> ApplyJobLevelAsync(SyncCommandBundle<CreateJobLevelIrisaSyncCommand, UpdateJobLevelIrisaSyncCommand, DeleteJobLevelIrisaSyncCommand> selectedBundle);
        Task<BatchResult<SyncResult>> SyncJobLevelAsync();
        #endregion

        #region OrganizationUnit
        Task<BatchResult<SyncCommandBundle<CreateOrganizationUnitIrisaSyncCommand, UpdateOrganizationUnitIrisaSyncCommand, DeleteOrganizationUnitIrisaSyncCommand>>> SyncOrganizationUnitPreviewAsync();
        Task<BatchResult<SyncResult>> ApplyOrganizationUnitAsync(SyncCommandBundle<CreateOrganizationUnitIrisaSyncCommand, UpdateOrganizationUnitIrisaSyncCommand, DeleteOrganizationUnitIrisaSyncCommand> selectedBundle);
        Task<BatchResult<SyncResult>> SyncOrganizationUnitAsync();
        #endregion

        #region Post
        Task<BatchResult<SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand>>> SyncPostPreviewAsync();
        Task<BatchResult<SyncResult>> ApplyPostAsync(SyncCommandBundle<CreatePostCommand, UpdatePostCommand, DeletePostCommand> selectedBundle);
        Task<BatchResult<SyncResult>> SyncPostAsync();
        #endregion

        #region Assignment
        Task<BatchResult<SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand>>> SyncAssignmentsPreviewAsync();
        Task<BatchResult<SyncResult>> ApplyAssignmentsAsync(SyncCommandBundle<CreateAssignmentCommand, UpdateAssignmentCommand, DeleteAssignmentCommand> selectedBundle);
        Task<BatchResult<SyncResult>> SyncAssignmentsAsync();
        #endregion
    }
}
