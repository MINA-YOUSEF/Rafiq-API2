using Rafiq.Application.DTOs.Sessions;

namespace Rafiq.Application.Interfaces.External;

public interface IAiAnalysisClient
{
    Task<AiAnalysisResponseDto> AnalyzeAsync(AiAnalysisRequestDto request, CancellationToken cancellationToken = default);
    Task<AiExtractReferenceResponseDto> ExtractReferenceAsync(
        AiExtractReferenceRequestDto request,
        CancellationToken cancellationToken = default);
    Task<AiAnalysisResponseDto> CompareAsync(
        AiCompareRequestDto request,
        CancellationToken cancellationToken = default);
}
