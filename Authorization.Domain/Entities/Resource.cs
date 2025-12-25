using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Resource : DataScopedEntity, IAggregateRoot
    {
        public string Key { get; private set; } // e.g. "Invoice", "SystemSettings"
        public string Name { get; private set; }
        public ResourceType Type { get; private set; }

        // سلسله مراتب ریسورس‌ها (مثل منوهای تودرتو)
        public Guid? ParentId { get; private set; }
        public string? ResourcePath { get; private set; } // "/Guid/Guid"

        // Navigation
        public virtual Resource? Parent { get; private set; }
        public virtual ICollection<Resource> Children { get; private set; } = new List<Resource>();

        // ارتباط مستقیم با پرمیشن‌ها
        public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

        protected Resource() { }

        public Resource(string key, string name, ResourceType type, Guid? parentId = null)
        {
            Key = key.Trim().ToUpperInvariant(); // کلید همیشه حروف بزرگ برای مقایسه راحت‌تر
            Name = name;
            Type = type;
            ParentId = parentId;
        }

        public void SetPath(string path) => ResourcePath = path;
    }
}
