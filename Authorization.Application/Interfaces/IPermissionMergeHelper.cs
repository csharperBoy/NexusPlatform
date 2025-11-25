using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    /*
     📌 IPermissionMergeHelper
     -------------------------
     وظیفه این سرویس تعریف "قانون ادغام" Permission ها است.

     🧠 چرا لازم داریم؟
     - دسترسی‌های یک کاربر ممکن است از چند منبع بیاید:
       ✔️ نقش‌ها
       ✔️ دسترسی صریح (Explicit)
       ✔️ ارث‌بری از Resource والد
     - باید مشخص شود در صورت تضاد، کدام اعمال شود:
       ❗ منع (Deny) نسبت به Allow اولویت دارد
       ❗ Explicit نسبت به Role اولویت دارد

     🛠 متدها:
     1. Merge
        - مجموعه‌ای از Permission ها را گرفته و یک خروجی نهایی تولید می‌کند.
    */

    public interface IPermissionMergeHelper
    {
        EffectivePermissionDto Merge(IEnumerable<PermissionDto> permissions);
    }
}
