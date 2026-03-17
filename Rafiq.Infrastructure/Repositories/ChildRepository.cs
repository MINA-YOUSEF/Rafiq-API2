using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Data;

namespace Rafiq.Infrastructure.Repositories;

public class ChildRepository : GenericRepository<Child>, IChildRepository
{
    public ChildRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Child?> GetByIdWithDetailsAsync(int childId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.ParentProfile)
            .Include(x => x.SpecialistProfile)
            .FirstOrDefaultAsync(x => x.Id == childId, cancellationToken);
    }

        public async Task<Child?> GetByIdForParentAsync(int childId, int parentUserId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(x => x.ParentProfile)
                .Include(x => x.SpecialistProfile)
                .FirstOrDefaultAsync(
                    x => x.Id == childId && x.ParentProfile.UserId == parentUserId,
                    cancellationToken);
        }

        public async Task<Child?> GetByIdForSpecialistAsync(int childId, int specialistUserId, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(x => x.ParentProfile)
                .Include(x => x.SpecialistProfile)
                .FirstOrDefaultAsync(
                    x => x.Id == childId && x.SpecialistProfile != null && x.SpecialistProfile.UserId == specialistUserId,
                    cancellationToken);
        }
}
