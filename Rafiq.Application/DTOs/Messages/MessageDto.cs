namespace Rafiq.Application.DTOs.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int SenderUserId { get; set; }
    public int ReceiverUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAtUtc { get; set; }
}
