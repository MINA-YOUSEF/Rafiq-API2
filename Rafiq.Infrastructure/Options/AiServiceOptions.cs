namespace Rafiq.Infrastructure.Options;

public class AiServiceOptions
{
    public const string SectionName = "AiService";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string? ExtractReferenceEndpoint { get; set; }
    public string? CompareEndpoint { get; set; }
    public bool UseReferenceComparison { get; set; } = false;
}
