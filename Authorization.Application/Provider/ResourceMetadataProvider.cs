using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Core.Domain.Attributes;
using Core.Domain.Common.EntityProperties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Authorization.Application.Provider
{
    public class ResourceMetadataProvider : IResourceMetadataProvider
    {
        public IReadOnlyList<ResourceMetadataDto> Resources { get; }

        public ResourceMetadataDto? GetMetadata(string resourceKey)
        {
            return Resources.FirstOrDefault(r => r.ResourceKey.Equals(resourceKey, StringComparison.OrdinalIgnoreCase));
        }
        private static List<FieldDto> GetScalarFields(IEntityType entityType)
        {
            return entityType.GetProperties()
                .Where(p => !p.IsForeignKey())
                .Where(p =>
                {
                    // فقط فیلدهایی را نگه دار که DisplayAttribute دارند
                    var displayAttr = p.PropertyInfo?.GetCustomAttribute<DisplayAttribute>();
                    return displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name);
                })
                .Select(p =>
                {
                    var displayAttr = p.PropertyInfo!.GetCustomAttribute<DisplayAttribute>();
                    return new FieldDto(
                        p.Name,
                        p.ClrType.BaseType?.Name,
                        p.IsNullable
                            ? p.ClrType.GenericTypeArguments.FirstOrDefault()?.Name ?? p.ClrType.Name
                            : p.ClrType.Name,
                        displayAttr!.Name  // حتماً وجود دارد
                    );
                })
                .ToList();
        }
        public ResourceMetadataProvider(IServiceProvider rootProvider, IEnumerable<Type> dbContextTypes)
        {
            var resources = new List<ResourceMetadataDto>();

            foreach (var dbContextType in dbContextTypes)
            {
                using var scope = rootProvider.CreateScope();
                var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);

                foreach (var entityType in dbContext.Model.GetEntityTypes())
                {
                    var scalarFields = GetScalarFields(entityType);

                    var joins = entityType.GetNavigations()
                        .Select(nav => new JoinDto(
                            nav.Name,
                            nav.TargetEntityType.ClrType.Name,
                            nav.ForeignKey.Properties.Select(p => p.Name).FirstOrDefault(),
                            nav.ForeignKey.PrincipalKey.Properties.Select(p => p.Name).FirstOrDefault(),
                            GetScalarFields(nav.TargetEntityType)
                        ))
                        .ToList();

                    // دریافت اتریبیوت سفارشی
                    var securedAttr = entityType.ClrType.GetCustomAttribute<SecuredResourceAttribute>();
                    var resourceKey = securedAttr?.ResourceKey ?? entityType.ClrType.Name;

                    // دریافت DynamicFilterableAttribute
                    var dynamicFilterAttr = entityType.ClrType.GetCustomAttribute<DynamicFilterableAttribute>();
                    bool useDynamicFilter = dynamicFilterAttr != null;
                    bool useNavigate = dynamicFilterAttr?.UseNavigation ?? false;
                    bool useScope = typeof(IOwnerableEntity).IsAssignableFrom(entityType.ClrType);

                    resources.Add(new ResourceMetadataDto(
                        resourceKey.ToLower(),
                        entityType.ClrType.Name,
                        useDynamicFilter,
                        useNavigate,
                        useScope,
                        scalarFields,
                        joins
                    ));
                }
            }

            Resources = resources.AsReadOnly();
        }
    }
}