using Rafiq.Domain.Common;

namespace Rafiq.Domain.Entities;

public class Message : BaseEntity
{
    public int ChildId { get; set; }
    public int SenderUserId { get; set; }
    public int ReceiverUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    public Child Child { get; set; } = null!;
}
