using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Messages;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task JoinChildConversation(int childId)
    {
        // Authorization and ownership are validated in the service call.
        await _messageService.GetConversationByChildAsync(childId, new PagedRequest { PageNumber = 1, PageSize = 1 });
        await Groups.AddToGroupAsync(Context.ConnectionId, BuildChildGroupName(childId));
    }

    public async Task LeaveChildConversation(int childId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildChildGroupName(childId));
    }

    public async Task SendMessage(SendMessageRequestDto request)
    {
        var message = await _messageService.SendAsync(request);
        await Clients.Group(BuildChildGroupName(request.ChildId)).SendAsync("ReceiveMessage", message);
    }

    private static string BuildChildGroupName(int childId)
    {
        return $"child-{childId}";
    }
}
