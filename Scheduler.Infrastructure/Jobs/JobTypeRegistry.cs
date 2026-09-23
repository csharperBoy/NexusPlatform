using Scheduler.Application.Abstractions;
using Scheduler.Application.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Infrastructure.Jobs
{
    /// <summary>
    /// رجیستری کلید → Type برای payloadهای job.
    /// - خودکار همه‌ی assemblyهای لود شده رو اسکن می‌کنه
    /// - می‌تونی با Register() دستی هم اضافه کنی
    /// </summary>
    public class JobTypeRegistry
    {
        private readonly Dictionary<string, Type> _byKey = new();
        private readonly HashSet<Assembly> _scanned = new();
        private readonly object _lock = new();
        private bool _scannedAll;

        /// <summary>ثبت دستی یک payload type (از AddScheduledJobHandler صدا زده میشه)</summary>
        public void Register(Type payloadType)
        {
            if (payloadType.IsAbstract || payloadType.IsInterface)
                throw new InvalidOperationException(
                    $"{payloadType.FullName} must be a concrete class");

            if (!typeof(IScheduledJobPayload).IsAssignableFrom(payloadType))
                throw new InvalidOperationException(
                    $"{payloadType.FullName} must implement {nameof(IScheduledJobPayload)}");

            var attr = payloadType.GetCustomAttribute<ScheduledJobAttribute>()
                ?? throw new InvalidOperationException(
                    $"{payloadType.FullName} must have [ScheduledJob] attribute");

            lock (_lock)
            {
                if (_byKey.TryGetValue(attr.Key, out var existing) && existing != payloadType)
                    throw new InvalidOperationException(
                        $"Duplicate ScheduledJob key '{attr.Key}': " +
                        $"{existing.FullName} vs {payloadType.FullName}");

                _byKey[attr.Key] = payloadType;
            }
        }

        /// <summary>کلید یکتا برای یک payload type</summary>
        public string GetKey(Type payloadType)
        {
            var attr = payloadType.GetCustomAttribute<ScheduledJobAttribute>()
                ?? throw new InvalidOperationException(
                    $"{payloadType.FullName} must have [ScheduledJob] attribute");
            return attr.Key;
        }

        /// <summary>جستجوی کلید → Type (با lazy scan)</summary>
        public Type? Resolve(string key)
        {
            if (_byKey.TryGetValue(key, out var t)) return t;

            // شاید assembly تازه لود شده — دوباره اسکن کن
            ScanAllAssemblies();

            return _byKey.GetValueOrDefault(key);
        }

        /// <summary>اسکن همه‌ی assemblyهای فعلی AppDomain</summary>
        public void ScanAllAssemblies()
        {
            if (_scannedAll) return;

            lock (_lock)
            {
                if (_scannedAll) return;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    ScanAssembly(asm);
                }
                _scannedAll = true;
            }
        }

        /// <summary>اسکن یک assembly مشخص</summary>
        public void ScanAssembly(Assembly assembly)
        {
            lock (_lock)
            {
                if (!_scanned.Add(assembly)) return;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // بعضی typeها لود نمیشن — بقیه رو بگیر
                types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }
            catch
            {
                return; // assembly مشکوک — skip
            }

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (!type.IsPublic && !type.IsNestedPublic) continue;
                if (!typeof(IScheduledJobPayload).IsAssignableFrom(type)) continue;

                // فقط typeهایی که [ScheduledJob] دارن
                var attr = type.GetCustomAttribute<ScheduledJobAttribute>();
                if (attr is null) continue;

                Register(type);
            }
        }
    }
}
