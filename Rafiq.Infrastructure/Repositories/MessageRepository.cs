using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Data;

namespace Rafiq.Infrastructure.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyCollection<Message>> GetConversationByChildAsync(
        int childId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.ChildId == childId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountConversationByChildAsync(int childId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(x => x.ChildId == childId, cancellationToken);
    }
}
