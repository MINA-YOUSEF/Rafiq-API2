namespace Rafiq.Application.DTOs.Media;

public class ProfileImageDto
{
    public int MediaId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}
