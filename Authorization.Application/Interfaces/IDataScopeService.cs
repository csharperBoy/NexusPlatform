using Authorization.Application.Commands.DataScopes;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    /*
     📌 IDataScopeService (Write Service)
     ------------------------------------
     سرویس مخصوص عملیات نوشتن روی DataScope:

     🔧 مسئولیت‌ها:
     - ایجاد یا تغییر DataScope
     - مدیریت کش پس از نوشتن
     - انجام Validations سطح سرویس

     مطابق الگوی تمپلیت تو، این سرویس:
     ✔️ Command Handler نیست  
     ✔️ یک API Service سطح بالا است  
     ✔️ Handler ها را صدا می‌زند  
    */

    public interface IDataScopeService
    {
        Task<Result> AssignDataScopeAsync(AssignDataScopeCommand cmd, CancellationToken ct = default);
        Task<Result> UpdateDataScopeAsync(UpdateDataScopeCommand cmd, CancellationToken ct = default);
    }
}
