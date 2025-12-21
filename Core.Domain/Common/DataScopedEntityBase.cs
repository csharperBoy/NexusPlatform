// Core.Domain/Common/DataScopedEntityBase.cs
using Core.Domain.Interfaces;
using Core.Shared.Extensions;

namespace Core.Domain.Common
{
    /// <summary>
    /// کلاس پایه برای تمام موجودیت‌های قابل فیلتر بر اساس DataScope
    /// دارای پیاده‌سازی کامل متدهای مدیریت مالکیت و واحد سازمانی
    /// </summary>
    public abstract class DataScopedEntityBase : AuditableEntity, IDataScopedEntity
    {
        // فیلدهای اصلی برای DataScope
        private Guid? _unitId;
        private Guid? _ownerId;
        private string? _path;
        private bool _pathCalculated = false;

        // Propertyهای IDataScopedEntity با پیاده‌سازی کامل
        public virtual Guid? UnitId
        {
            get => _unitId;
            protected set => _unitId = value;
        }

        public virtual Guid? OwnerId
        {
            get => _ownerId;
            protected set => _ownerId = value;
        }

        public virtual string? Path
        {
            get
            {
                // اگر Path محاسبه نشده و نیاز به محاسبه خودکار داریم
                if (!_pathCalculated && ShouldCalculatePathAutomatically())
                {
                    CalculatePath();
                }
                return _path;
            }
            protected set
            {
                _path = value;
                _pathCalculated = true;
            }
        }

        // پیاده‌سازی property IsPersonal
        public virtual bool IsPersonal => GetEffectiveOwnerId().HasValue;

        // متدهای مدیریت مالکیت (پیاده‌سازی کامل)
        public virtual void SetOwnership(Guid ownerId, bool overrideExisting = false)
        {
            if (overrideExisting || !OwnerId.HasValue)
            {
                OwnerId = ownerId;
                ModifiedAt = DateTime.UtcNow;

                // اگر CreatedBy هم تنظیم نشده، آن را هم تنظیم کن
                if (string.IsNullOrEmpty(CreatedBy))
                {
                    SetCreatedBy(ownerId);
                }
            }
        }

        public virtual void SetOwnership(string owner, bool overrideExisting = false)
        {
            var ownerGuid = owner.TryParseGuid();
            if (ownerGuid.HasValue)
            {
                SetOwnership(ownerGuid.Value, overrideExisting);
            }
        }

        public virtual void ClearOwnership()
        {
            OwnerId = null;
            ModifiedAt = DateTime.UtcNow;
        }

        // متدهای مدیریت واحد سازمانی (پیاده‌سازی کامل)
        public virtual void SetUnit(Guid unitId, bool overrideExisting = false)
        {
            if (overrideExisting || !UnitId.HasValue)
            {
                UnitId = unitId;
                ModifiedAt = DateTime.UtcNow;
            }
        }

        public virtual void ClearUnit()
        {
            UnitId = null;
            ModifiedAt = DateTime.UtcNow;
        }

        // متدهای مدیریت Path (پیاده‌سازی کامل)
        public virtual void CalculatePath(string parentPath = "")
        {
            if (Id == Guid.Empty)
            {
                throw new InvalidOperationException("Entity must have an Id before calculating path");
            }

            Path = string.IsNullOrEmpty(parentPath)
                ? $"/{Id}"
                : $"{parentPath}/{Id}";

            _pathCalculated = true;
        }

        public virtual void UpdatePath(string newPath)
        {
            Path = newPath;
            ModifiedAt = DateTime.UtcNow;
        }

        // پیاده‌سازی متدهای IDataScopedEntity
        public virtual Guid? GetCreatedById()
        {
            return CreatedBy?.TryParseGuid();
        }

        public virtual Guid? GetEffectiveOwnerId()
        {
            return OwnerId ?? GetCreatedById();
        }

        // متدهای بررسی (پیاده‌سازی کامل)
        public virtual bool HasOwnership()
        {
            return OwnerId.HasValue || GetCreatedById().HasValue;
        }

        public virtual bool HasUnit()
        {
            return UnitId.HasValue;
        }

        public virtual bool IsOwnedBy(Guid userId)
        {
            var effectiveOwner = GetEffectiveOwnerId();
            return effectiveOwner == userId;
        }

        public virtual bool IsInUnit(Guid unitId)
        {
            return UnitId == unitId;
        }

        // پیاده‌سازی متدهای اضافی از interface
        public virtual bool BelongsToUnit(Guid unitId)
        {
            return UnitId == unitId;
        }

        public virtual bool BelongsToUser(Guid userId)
        {
            return GetEffectiveOwnerId() == userId;
        }

        public virtual bool IsInPath(string pathPrefix)
        {
            if (string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(pathPrefix))
                return false;

            return Path.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase);
        }

        // متدهای virtual برای سفارشی‌سازی
        protected virtual bool ShouldCalculatePathAutomatically()
        {
            return true; // پیش‌فرض: Path به صورت خودکار محاسبه شود
        }

        protected virtual void OnOwnershipChanged(Guid? oldOwnerId, Guid? newOwnerId)
        {
            // می‌توان در کلاس‌های مشتق شده override کرد
        }

        protected virtual void OnUnitChanged(Guid? oldUnitId, Guid? newUnitId)
        {
            // می‌توان در کلاس‌های مشتق شده override کرد
        }
    }
}