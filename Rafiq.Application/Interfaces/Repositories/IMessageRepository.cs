using Rafiq.Domain.Entities;

namespace Rafiq.Application.Interfaces.Repositories;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IReadOnlyCollection<Message>> GetConversationByChildAsync(
        int childId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountConversationByChildAsync(int childId, CancellationToken cancellationToken = default);
}
