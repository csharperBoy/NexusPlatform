using Core.Domain.Specifications;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Specifications
{
    /// <summary>
    /// Specification برای گرفتن واحدهای سازمانی بر اساس لیستی از شناسه‌ها
    /// </summary>
    public class OrganizationUnitByIdsSpec : BaseSpecification<OrganizationUnit>
    {
        public OrganizationUnitByIdsSpec(IEnumerable<Guid> unitIds)
            : base(u => unitIds.Contains(u.Id))
        {
            AddInclude(u => u.Parent);
            ApplyOrderBy(u => u.Name);
        }
    }
}
