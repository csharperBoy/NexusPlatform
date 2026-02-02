using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Models
{
    public class ModuleSettings
    {
        public List<ModuleItem> Enabled { get; set; } = new List<ModuleItem>();

        // متد کمکی برای بررسی فعال بودن ماژول
        public bool IsModuleActive(string moduleName)
        {
            return Enabled.Any(m =>
                m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        }

        // متد کمکی برای گرفتن ترتیب ماژول
        public int? GetModuleOrder(string moduleName)
        {
            return Enabled.FirstOrDefault(m =>
                m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))?.Order;
        }
    }
}
