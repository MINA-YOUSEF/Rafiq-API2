using Rafiq.Domain.Entities;

namespace Rafiq.Application.Interfaces.Repositories;

public interface IChildRepository : IGenericRepository<Child>
{
    Task<Child?> GetByIdWithDetailsAsync(int childId, CancellationToken cancellationToken = default);
    Task<Child?> GetByIdForParentAsync(int childId, int parentUserId, CancellationToken cancellationToken = default);
    Task<Child?> GetByIdForSpecialistAsync(int childId, int specialistUserId, CancellationToken cancellationToken = default);
}
