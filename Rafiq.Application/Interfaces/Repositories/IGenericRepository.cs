using System.Linq.Expressions;
using Rafiq.Domain.Common;

namespace Rafiq.Application.Interfaces.Repositories;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Query();
    Task<IReadOnlyCollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
