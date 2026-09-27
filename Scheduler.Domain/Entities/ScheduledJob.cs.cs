using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Scheduler.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Domain.Entities
{
    public class ScheduledJob : BaseEntity , IAuditableEntity , IOwnerableEntity
    {

        #region impelement IOwnerableEntity

        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }
        public Guid? OwnerUserId { get; protected set; }

        public void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerUserId = userId;
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetUserOwner(Guid userId)
        {
            OwnerUserId = userId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }

        #endregion

        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

        public void Touch() => ModifiedAt = DateTime.UtcNow;
        #endregion


        /// <summary>کلید یکتا برای نوع payload (از ScheduledJobAttribute)</summary>
        public string JobType { get; private set; } = default!;

        /// <summary>شناسه‌ی job در Hangfire</summary>
        public string HangfireJobId { get; private set; } = default!;

        /// <summary>Payload serialized (JSON)</summary>
        public string PayloadJson { get; private set; } = default!;

        /// <summary>زمان هدف fire (به UTC)</summary>
        public DateTimeOffset FireAt { get; private set; }

        /// <summary>پیش‌افتادگی (زمانی که Hangfire trigger میشه)</summary>
        public TimeSpan PreFireBuffer { get; private set; }

        public ScheduledJobState State { get; private set; }
        public DateTimeOffset? StartedAt { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }
        public string? Error { get; private set; }

        private ScheduledJob() { }  // EF Core

        public static ScheduledJob Create(
            string jobType,
            string hangfireJobId,
            string payloadJson,
            DateTimeOffset fireAt,
            TimeSpan preFireBuffer)
        {
            return new ScheduledJob
            {
                JobType = jobType,
                HangfireJobId = hangfireJobId,
                PayloadJson = payloadJson,
                FireAt = fireAt,
                PreFireBuffer = preFireBuffer,
                State = ScheduledJobState.Pending,
            };
        }

        public void MarkExecuting()
        {
            State = ScheduledJobState.Executing;
            StartedAt = DateTimeOffset.UtcNow;
        }

        public void MarkSucceeded()
        {
            State = ScheduledJobState.Succeeded;
            CompletedAt = DateTimeOffset.UtcNow;
        }

        public void MarkFailed(string error)
        {
            State = ScheduledJobState.Failed;
            CompletedAt = DateTimeOffset.UtcNow;
            Error = error;
        }

        public void MarkCancelled()
        {
            State = ScheduledJobState.Cancelled;
            CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}
