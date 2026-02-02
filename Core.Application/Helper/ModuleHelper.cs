using Core.Application.Models;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Helper
{
    public static class ModuleHelper
    {
        private static ModuleSettings _settings;

        public static void Initialize(ModuleSettings settings)
        {
            _settings = settings ??
                throw new ArgumentNullException(nameof(settings));
        }

        public static bool IsActive(ModuleEnum module)
        {
            if (_settings == null)
                throw new InvalidOperationException(
                    "ModuleHelper has not been initialized. Call Initialize() first.");

            var moduleName = module.ToString();
            return _settings.IsModuleActive(moduleName);
        }

        // متد اضافی: گرفتن ترتیب ماژول
        public static int? GetOrder(ModuleEnum module)
        {
            if (_settings == null)
                throw new InvalidOperationException(
                    "ModuleHelper has not been initialized. Call Initialize() first.");

            var moduleName = module.ToString();
            return _settings.GetModuleOrder(moduleName);
        }

        // متد اضافی: گرفتن لیست ماژول‌های فعال
        public static List<ModuleEnum> GetActiveModules()
        {
            if (_settings == null)
                throw new InvalidOperationException(
                    "ModuleHelper has not been initialized. Call Initialize() first.");

            var activeModules = new List<ModuleEnum>();

            foreach (var moduleItem in _settings.Enabled)
            {
                if (Enum.TryParse<ModuleEnum>(moduleItem.Name, true, out var module))
                {
                    activeModules.Add(module);
                }
            }

            return activeModules;
        }

        // متد اضافی: گرفتن ماژول‌های فعال به همراه ترتیب
        public static Dictionary<ModuleEnum, int> GetActiveModulesWithOrder()
        {
            if (_settings == null)
                throw new InvalidOperationException(
                    "ModuleHelper has not been initialized. Call Initialize() first.");

            var result = new Dictionary<ModuleEnum, int>();

            foreach (var moduleItem in _settings.Enabled)
            {
                if (Enum.TryParse<ModuleEnum>(moduleItem.Name, true, out var module))
                {
                    result[module] = moduleItem.Order;
                }
            }

            return result;
        }
    }
}
