using Core.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.Application.Abstractions
{
    public interface ISpecificationRepository< TEntity, TKey>
    
           where TEntity : class
        where TKey : IEquatable<TKey>
    {
        Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> specification);
        Task<IEnumerable<TEntity>> ListBySpecAsync(ISpecification<TEntity> specification);
        Task<(IEnumerable<TEntity> Items, int TotalCount)> FindBySpecAsync(ISpecification<TEntity> specification);
        Task<int> CountBySpecAsync(ISpecification<TEntity> specification);


    }
}