using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using EPM.Application.Abstractions;
using EPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EPM.Infrastructure.Data;

public class EpmUnitOfWork : EfUnitOfWork<EpmDbContext>, IEpmUnitOfWork<EpmDbContext>
{
    public EpmUnitOfWork(
        IRepository<EpmDbContext, User, Guid> users,
        IRepository<EpmDbContext, Category, Guid> categories,
        IRepository<EpmDbContext, Indicator, Guid> indicators,
        IRepository<EpmDbContext, IndicatorCategory, Guid> indicatorCategories,
        IRepository<EpmDbContext, IndicatorValue, Guid> indicatorValues,
        IRepository<EpmDbContext, ClaimUserOnIndicator, Guid> claimUserOnIndicators,
        IRepository<EpmDbContext, LookUp, Guid> lookUps,
        IRepository<EpmDbContext, LookUpDestination, Guid> lookUpDestinations,
        IRepository<EpmDbContext, LookUpValue, Guid> lookUpValues,
        IRepository<EpmDbContext, MapItem, Guid> mapItems,

        ISpecificationRepository<User, Guid> userSpecs,
        ISpecificationRepository<Category, Guid> categorySpecs,
        ISpecificationRepository<Indicator, Guid> indicatorSpecs,
        ISpecificationRepository<IndicatorCategory, Guid> indicatorCategorySpecs,
        ISpecificationRepository<IndicatorValue, Guid> indicatorValueSpecs,
        ISpecificationRepository<ClaimUserOnIndicator, Guid> claimUserOnIndicatorSpecs,
        ISpecificationRepository<LookUp, Guid> lookUpSpecs,
        ISpecificationRepository<LookUpDestination, Guid> lookUpDestinationSpecs,
        ISpecificationRepository<LookUpValue, Guid> lookUpValueSpecs,
        ISpecificationRepository<MapItem, Guid> mapItemSpecs,
        EpmDbContext dbContext,
        IOutboxService<EpmDbContext> outboxService,
        ILogger<EfUnitOfWork<EpmDbContext>> logger)
        : base(dbContext, outboxService, logger)
    {
        
    }
  public  IRepository<EpmDbContext, User, Guid> Users{get;}
  public  IRepository<EpmDbContext, Category, Guid> Categories{get;}
  public  IRepository<EpmDbContext, Indicator, Guid> Indicators{get;}
  public  IRepository<EpmDbContext, IndicatorCategory, Guid> IndicatorCategories{get;}
  public  IRepository<EpmDbContext, IndicatorValue, Guid> IndicatorValues{get;}
  public  IRepository<EpmDbContext, ClaimUserOnIndicator, Guid> ClaimUserOnIndicators{get;}
  public  IRepository<EpmDbContext, LookUp, Guid> LookUps{get;}
  public  IRepository<EpmDbContext, LookUpDestination, Guid> LookUpDestinations{get;}
  public  IRepository<EpmDbContext, LookUpValue, Guid> LookUpValues{get;}
  public IRepository<EpmDbContext, MapItem, Guid> MapItems { get; }



  public  ISpecificationRepository<User, Guid> UserSpecs{get;}
  public      ISpecificationRepository<Category, Guid> CategorySpecs{get;}
  public      ISpecificationRepository<Indicator, Guid> IndicatorSpecs{get;}
  public      ISpecificationRepository<IndicatorCategory, Guid> IndicatorCategorySpecs{get;}
  public      ISpecificationRepository<IndicatorValue, Guid> IndicatorValueSpecs{get;}
  public      ISpecificationRepository<ClaimUserOnIndicator, Guid> ClaimUserOnIndicatorSpecs{get;}
  public      ISpecificationRepository<LookUp, Guid> LookUpSpecs{get;}
  public      ISpecificationRepository<LookUpDestination, Guid> LookUpDestinationSpecs{get;}
  public      ISpecificationRepository<LookUpValue, Guid> LookUpValueSpecs{get;}
  public ISpecificationRepository<MapItem, Guid> MapItemSpecs { get; }


}