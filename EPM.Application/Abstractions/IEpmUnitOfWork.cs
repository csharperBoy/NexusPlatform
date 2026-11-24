using Core.Application.Abstractions;
using EPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EPM.Application.Abstractions;

public interface IEpmUnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
    where TDbContext : DbContext
{

    IRepository<TDbContext, User, Guid> Users { get; }
    IRepository<TDbContext, Category, Guid> Categories { get; }
    IRepository<TDbContext, Indicator, Guid> Indicators { get; }
    IRepository<TDbContext, IndicatorCategory, Guid> IndicatorCategories { get; }
    IRepository<TDbContext, IndicatorValue, Guid> IndicatorValues { get; }
    IRepository<TDbContext, ClaimUserOnIndicator, Guid> ClaimUserOnIndicators { get; }
    IRepository<TDbContext, LookUp, Guid> LookUps { get; }
    IRepository<TDbContext, LookUpDestination, Guid> LookUpDestinations { get; }
    IRepository<TDbContext, LookUpValue, Guid> LookUpValues { get; }
    IRepository<TDbContext, MapItem, Guid> MapItems { get; }



    ISpecificationRepository<User, Guid> UserSpecs { get; }
    ISpecificationRepository<Category, Guid> CategorySpecs { get; }
    ISpecificationRepository<Indicator, Guid> IndicatorSpecs { get; }
    ISpecificationRepository<IndicatorCategory, Guid> IndicatorCategorySpecs { get; }
    ISpecificationRepository<IndicatorValue, Guid> IndicatorValueSpecs { get; }
    ISpecificationRepository<ClaimUserOnIndicator, Guid> ClaimUserOnIndicatorSpecs { get; }
    ISpecificationRepository<LookUp, Guid> LookUpSpecs { get; }
    ISpecificationRepository<LookUpDestination, Guid> LookUpDestinationSpecs { get; }
    ISpecificationRepository<LookUpValue, Guid> LookUpValueSpecs { get; }
    ISpecificationRepository<MapItem, Guid> MapItemSpecs { get; }
   
}