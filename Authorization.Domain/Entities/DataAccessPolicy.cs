using Authorization.Domain.ValueObjects;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{/// <summary>
/// موجودیت DataAccessPolicy برای کنترل دسترسی سطح داده
/// </summary>
    public class DataAccessPolicy : AuditableEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ResourceType ResourceType { get; private set; }
        public string EntityType { get; private set; } // نوع موجودیت هدف (مثلاً Sales, Employee, etc.)

        // محدوده‌های دسترسی
        public OrganizationalScope OrganizationalScope { get; private set; }
        public GeographicScope GeographicScope { get; private set; }
        public TemporalScope TemporalScope { get; private set; }
        public HierarchicalScope HierarchicalScope { get; private set; }

        public bool IsActive { get; private set; }

        private DataAccessPolicy() { } // برای EF Core

        public DataAccessPolicy(string name, ResourceType resourceType, string entityType,
                              OrganizationalScope orgScope, string description = null)
        {
            Name = name;
            ResourceType = resourceType;
            EntityType = entityType;
            OrganizationalScope = orgScope;
            Description = description;
            IsActive = true;
        }

        public void UpdateScopes(OrganizationalScope orgScope, GeographicScope geoScope = null,
                               TemporalScope tempScope = null, HierarchicalScope hierScope = null)
        {
            OrganizationalScope = orgScope;
            GeographicScope = geoScope;
            TemporalScope = tempScope;
            HierarchicalScope = hierScope;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
}
