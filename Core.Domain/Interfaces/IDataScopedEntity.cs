using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
        /// <summary>
        /// موجودیت‌هایی که می‌توانند بر اساس محدوده داده فیلتر شوند
        /// </summary>
        public interface IDataScopedEntity
        {
            Guid? UnitId { get; }
            Guid? OwnerId { get; }
            string? Path { get; }

            // Propertyهای اضافی
            bool IsPersonal { get; }

            // متدهای مدیریتی
            void SetOwnership(Guid ownerId, bool overrideExisting = false);
            void SetOwnership(string owner, bool overrideExisting = false);
            void ClearOwnership();

            void SetUnit(Guid unitId, bool overrideExisting = false);
            void ClearUnit();

            void CalculatePath(string parentPath = "");
            void UpdatePath(string newPath);

            // متدهای دسترسی
            Guid? GetCreatedById();
            Guid? GetEffectiveOwnerId();

            // متدهای بررسی
            bool HasOwnership();
            bool HasUnit();
            bool IsOwnedBy(Guid userId);
            bool IsInUnit(Guid unitId);
            bool BelongsToUnit(Guid unitId);
            bool BelongsToUser(Guid userId);
            bool IsInPath(string pathPrefix);
        }
    
    /// <summary>
    /// Extension methods برای IDataScopedEntity
    /// </summary>
    public static class DataScopedEntityExtensions
    {
        public static Guid? GetEffectiveOwnerId(this IDataScopedEntity entity)
        {
            return entity.OwnerId ?? entity.GetCreatedById();
        }

        public static bool IsPersonal(this IDataScopedEntity entity)
        {
            return entity.GetEffectiveOwnerId().HasValue;
        }

        public static bool BelongsToUnit(this IDataScopedEntity entity, Guid unitId)
        {
            return entity.UnitId == unitId;
        }

        public static bool BelongsToUser(this IDataScopedEntity entity, Guid userId)
        {
            return entity.GetEffectiveOwnerId() == userId;
        }

        public static bool IsInPath(this IDataScopedEntity entity, string pathPrefix)
        {
            if (string.IsNullOrEmpty(entity.Path) || string.IsNullOrEmpty(pathPrefix))
                return false;

            return entity.Path.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static int GetPathLevel(this IDataScopedEntity entity)
        {
            if (string.IsNullOrEmpty(entity.Path))
                return 0;

            return entity.Path.Count(c => c == '/') - 1;
        }
    }
}
