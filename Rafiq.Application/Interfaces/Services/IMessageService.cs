using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Messages;

namespace Rafiq.Application.Interfaces.Services;

public interface IMessageService
{
    Task<MessageDto> SendAsync(SendMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResult<MessageDto>> GetConversationByChildAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default);
}
