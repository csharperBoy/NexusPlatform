using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Domain.Enums;
using Core.Domain.Interfaces;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Specifications
{
    /// <summary>
    /// Specification برای فیلتر کردن موجودیت‌ها بر اساس DataScope
    /// </summary>
    /// <typeparam name="T">نوع موجودیت که باید IDataScopedEntity را پیاده‌سازی کند</typeparam>
    public class DataScopeSpecification<T> : BaseSpecification<T>
        where T : class, IDataScopedEntity
    {
        /// <summary>
        /// Constructor برای زمانی که DataScope مشخص است
        /// </summary>
        public DataScopeSpecification(
            Guid userId,
            DataScopeDto dataScope,
            IOrganizationService organizationService)
            : base(BuildCriteria(userId, dataScope, organizationService))
        {
            // می‌توانیم Includes یا OrderByها را اینجا اضافه کنیم
        }

        /// <summary>
        /// Constructor برای زمانی که فقط resourceKey داریم
        /// </summary>
        public DataScopeSpecification(
            Guid userId,
            string resourceKey,
            IDataScopeEvaluator dataScopeEvaluator,
            IOrganizationService organizationService)
            : this(
                  userId,
                  dataScopeEvaluator.EvaluateDataScopeAsync(userId, resourceKey).Result,
                  organizationService)
        {
        }

        /// <summary>
        /// متد برای ساخت Expression شرط بر اساس DataScope
        /// </summary>
        private static Expression<Func<T, bool>> BuildCriteria(
            Guid userId,
            DataScopeDto dataScope,
            IOrganizationService organizationService)
        {
            if (dataScope == null)
            {
                // پیش‌فرض: فقط داده‌های خود کاربر
                return e => e.GetEffectiveOwnerId() == userId;
            }

            return dataScope.Scope switch
            {
                ScopeType.All => e => true, // دسترسی به همه

                ScopeType.Self => e => e.GetEffectiveOwnerId() == userId, // فقط خود کاربر

                ScopeType.Unit => BuildUnitCriteria(userId, organizationService),

                ScopeType.SpecificUnit => BuildSpecificUnitCriteria(dataScope.SpecificUnitId),

                ScopeType.Subtree => BuildSubtreeCriteria(userId, dataScope, organizationService),

                _ => e => false // پیش‌فرض: هیچ دسترسی
            };
        }

        /// <summary>
        /// ساخت شرط برای ScopeType.Unit
        /// </summary>
        private static Expression<Func<T, bool>> BuildUnitCriteria(
            Guid userId,
            IOrganizationService organizationService)
        {
            var userUnits = organizationService.GetUserUnits(userId).Result;

            if (userUnits == null || !userUnits.Any())
                return e => false; // اگر کاربر هیچ واحدی ندارد

            return e => e.UnitId.HasValue && userUnits.Contains(e.UnitId.Value);
        }

        /// <summary>
        /// ساخت شرط برای ScopeType.SpecificUnit
        /// </summary>
        private static Expression<Func<T, bool>> BuildSpecificUnitCriteria(Guid? specificUnitId)
        {
            if (!specificUnitId.HasValue)
                return e => false;

            return e => e.UnitId == specificUnitId;
        }

        /// <summary>
        /// ساخت شرط برای ScopeType.Subtree
        /// </summary>
        private static Expression<Func<T, bool>> BuildSubtreeCriteria(
            Guid userId,
            DataScopeDto dataScope,
            IOrganizationService organizationService)
        {
            Guid? rootUnitId = dataScope.SpecificUnitId;

            // اگر SpecificUnitId ندارد، از واحد کاربر استفاده کن
            if (!rootUnitId.HasValue)
            {
                var userPosition = organizationService.GetUserPosition(userId).Result;
                rootUnitId = userPosition?.OrganizationUnitId;
            }

            if (!rootUnitId.HasValue)
                return e => false; // اگر واحد ریشه مشخص نیست

            var subtreeUnits = organizationService.GetSubtreeUnits(rootUnitId.Value, dataScope.Depth).Result;

            if (subtreeUnits == null || !subtreeUnits.Any())
                return e => false;

            return e => e.UnitId.HasValue && subtreeUnits.Contains(e.UnitId.Value);
        }

        // متدهای کمکی برای اضافه کردن Includes
        public DataScopeSpecification<T> IncludeUnit()
        {
            // اگر موجودیت Unit را به عنوان Navigation دارد
            // AddInclude(e => e.Unit);
            return this;
        }

        public DataScopeSpecification<T> IncludeOwner()
        {
            // اگر موجودیت Owner را به عنوان Navigation دارد
            // AddInclude(e => e.Owner);
            return this;
        }
    }
}
