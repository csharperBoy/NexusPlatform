using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common.EntityProperties
{
    public interface IAuditableEntity
    {
        public DateTime CreatedAt { get; set; } // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }


        void Touch();


        /* impelement
         public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        
        public void Touch() => ModifiedAt = DateTime.UtcNow;


         */
    }

    public static class AuditableEntityExtention
    {
        public static void SetCreateAtNow(this IAuditableEntity entity)
        {
             entity.CreatedAt = DateTime.Now;
        }
       
    }
}
