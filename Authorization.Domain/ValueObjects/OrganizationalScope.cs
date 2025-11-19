using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای محدوده سازمانی
    /// </summary>
    public class OrganizationalScope : ValueObject
    {
        public Guid? DepartmentId { get; }
        public Guid? UnitId { get; }
        public Guid? TeamId { get; }
        public bool IncludeSubDepartments { get; }
        public OrganizationalScope(Guid? departmentId = null, Guid? unitId = null,
                         Guid? teamId = null, bool includeSubDepartments = false)
        {
            DepartmentId = departmentId;
            UnitId = unitId;
            TeamId = teamId;
            IncludeSubDepartments = includeSubDepartments;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DepartmentId ?? Guid.Empty;
            yield return UnitId ?? Guid.Empty;
            yield return TeamId ?? Guid.Empty;
            yield return IncludeSubDepartments;
        }
    }
}
}
