using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Messages;
using Rafiq.Application.Interfaces.Services;

namespace Rafiq.API.Controllers.V1;

[Authorize]
public class MessagesController : BaseV1Controller
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> Send([FromBody] SendMessageRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _messageService.SendAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("child/{childId:int}")]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetConversation(
        int childId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _messageService.GetConversationByChildAsync(childId, request, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{messageId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int messageId, CancellationToken cancellationToken)
    {
        await _messageService.MarkAsReadAsync(messageId, cancellationToken);
        return NoContent();
    }
}
