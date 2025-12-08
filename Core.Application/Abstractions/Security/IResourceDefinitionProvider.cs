using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Security
{
    /// <summary>
    /// رابط برای تعریف منابع ماژول‌ها
    /// هر ماژول می‌تواند منابع خود را از طریق این رابط ثبت کند
    /// </summary>
    public interface IResourceDefinitionProvider
    {
        /// <summary>
        /// کلید منحصربه‌فرد ماژول
        /// </summary>
        string ModuleKey { get; }

        /// <summary>
        /// نام ماژول
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// منابع ماژول را برمی‌گرداند
        /// </summary>
        IEnumerable<ResourceDefinition> GetResourceDefinitions();
    }
}
