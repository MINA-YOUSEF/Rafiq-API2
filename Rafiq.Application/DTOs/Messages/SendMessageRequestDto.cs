namespace Rafiq.Application.DTOs.Messages;

public class SendMessageRequestDto
{
    public int ChildId { get; set; }
    public int ReceiverUserId { get; set; }
    public string Content { get; set; } = string.Empty;
}
