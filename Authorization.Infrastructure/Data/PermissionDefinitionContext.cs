using Authorization.Application.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Data
{
    internal class PermissionDefinitionContext : IPermissionDefinitionContext
    {
        private readonly ConcurrentBag<PermissionDescriptor> _bag;

        public PermissionDefinitionContext(ConcurrentBag<PermissionDescriptor> bag) => _bag = bag;

        public IModuleBuilder AddModule(string moduleCode, string name) => new ModuleBuilder(moduleCode, _bag);

        private class ModuleBuilder : IModuleBuilder
        {
            private readonly string _moduleCode;
            private readonly ConcurrentBag<PermissionDescriptor> _bag;
            public ModuleBuilder(string moduleCode, ConcurrentBag<PermissionDescriptor> bag)
            {
                _moduleCode = moduleCode;
                _bag = bag;
            }
            public IPageBuilder AddPage(string pageCode, string name) => new PageBuilder($"{_moduleCode}.{pageCode}", _bag);
        }

        private class PageBuilder : IPageBuilder
        {
            private readonly string _pageCode;
            private readonly ConcurrentBag<PermissionDescriptor> _bag;
            public PageBuilder(string pageCode, ConcurrentBag<PermissionDescriptor> bag)
            {
                _pageCode = pageCode;
                _bag = bag;
            }
            public IActionBuilder AddAction(string actionCode, string name) => new ActionBuilder($"{_pageCode}.{actionCode}", name, _bag);
        }

        private class ActionBuilder : IActionBuilder
        {
            private readonly string _actionCode;
            private readonly string _name;
            private readonly ConcurrentBag<PermissionDescriptor> _bag;
            public ActionBuilder(string actionCode, string name, ConcurrentBag<PermissionDescriptor> bag)
            {
                _actionCode = actionCode;
                _name = name;
                _bag = bag;
            }

            public IActionBuilder Add(string actionCode, string name, string? scope = null)
            {
                var code = $"{_actionCode}.{actionCode}";
                _bag.Add(new PermissionDescriptor(code, name, _actionCode, scope));
                return this;
            }
        }
    }
}
