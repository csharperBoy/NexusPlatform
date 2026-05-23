using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Core.Domain.Attributes;
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

        public ResourceMetadataProvider(IServiceProvider rootProvider, IEnumerable<Type> dbContextTypes)
        {
            var resources = new List<ResourceMetadataDto>();

            foreach (var dbContextType in dbContextTypes)
            {
                // از scoped provider استفاده می‌کنیم تا فقط درون scope داده‌ها بگیرند
                using var scope = rootProvider.CreateScope();
                var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);

                foreach (var entityType in dbContext.Model.GetEntityTypes())
                {
                    var scalarFieldst = entityType.GetProperties()
                       .Where(p => !p.IsForeignKey())
                       .ToList();

                    // فیلدهای اسکلر
                    var scalarFields = entityType.GetProperties()
                        .Where(p => !p.IsForeignKey())
                        .Select(p => new FieldDto(
                           p.Name ,
                           p.ClrType.BaseType?.Name,
                           p.IsNullable ? p.ClrType.GenericTypeArguments.FirstOrDefault()?.Name ?? p.ClrType.Name : p.ClrType.Name,
                           $"{p.Name}" 
                           //p.GetCustomAttribute<DisplayAttribute>()?.Name
                            )
                        )
                        .ToList();

                    // ارتباطات
                    var joins = entityType.GetNavigations()
                        .Select(nav => new JoinDto(
                            nav.Name,
                            nav.TargetEntityType.ClrType.Name,
                            nav.ForeignKey.Properties.Select(p => p.Name).FirstOrDefault(),
                            nav.ForeignKey.PrincipalKey.Properties.Select(p => p.Name).FirstOrDefault()))
                        .ToList();

                    // کلید منبع
                    var securedAttr = entityType.ClrType.GetCustomAttribute<SecuredResourceAttribute>();
                    var resourceKey = securedAttr?.ResourceKey ?? entityType.ClrType.Name;

                    resources.Add(new ResourceMetadataDto(
                        resourceKey,
                        entityType.ClrType.Name,
                        scalarFields,
                        joins));
                }
            }

            Resources = resources.AsReadOnly();
        }
    }

}
