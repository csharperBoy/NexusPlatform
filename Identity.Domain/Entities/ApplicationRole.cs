using Core.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    /*
     اضافه شدن توضیحات و ترتیب نقش‌ها (برای پنل مدیریت نقش‌ها).

    قابل گسترش برای سیستم سلسله‌مراتب نقش‌ها در آینده.
     */
    public class ApplicationRole : IdentityRole<Guid>, IAggregateRoot
    {
        public string? Description { get; private set; }
        public int? OrderNum { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        protected ApplicationRole() : base() { }

        public ApplicationRole(string name, string? description = null, int? orderNum = null)
            : base(name)
        {
            Description = description;
            OrderNum = orderNum;
            NormalizedName = name.ToUpperInvariant();
        }

        public void Update(string? description, int? orderNum)
        {
            Description = description;
            OrderNum = orderNum;
        }
    }
}