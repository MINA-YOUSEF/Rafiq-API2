using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Rafiq.Application.DTOs.Sessions;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.External;
using Rafiq.Infrastructure.Options;

namespace Rafiq.Infrastructure.Services;

public class AiAnalysisClient : IAiAnalysisClient
{
    private readonly HttpClient _httpClient;
    private readonly string _extractReferenceEndpoint;
    private readonly string _compareEndpoint;

    public AiAnalysisClient(HttpClient httpClient, IOptions<AiServiceOptions> aiOptions)
    {
        _httpClient = httpClient;
        _extractReferenceEndpoint = string.IsNullOrWhiteSpace(aiOptions.Value.ExtractReferenceEndpoint)
            ? "/extract-reference"
            : aiOptions.Value.ExtractReferenceEndpoint!;
        _compareEndpoint = string.IsNullOrWhiteSpace(aiOptions.Value.CompareEndpoint)
            ? "/compare"
            : aiOptions.Value.CompareEndpoint!;
    }

    public async Task<AiAnalysisResponseDto> AnalyzeAsync(AiAnalysisRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/analyze", new
        {
            videoUrl = request.VideoUrl,
            exerciseType = request.ExerciseType,
            expectedReps = request.ExpectedReps
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException($"AI analysis failed: {(int)response.StatusCode} {errorBody}");
        }

        var payload = await response.Content.ReadFromJsonAsync<AiResponseContract>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new BadRequestException("AI service returned an empty response.");
        }

        return new AiAnalysisResponseDto
        {
            AccuracyScore = payload.AccuracyScore,
            RepetitionCount = payload.RepetitionCount,
            MistakeCount = payload.MistakeCount,
            Feedback = payload.Feedback ?? string.Empty,
            JointAngles = payload.JointAngles.ValueKind == JsonValueKind.Undefined
                ? "{}"
                : payload.JointAngles.GetRawText()
        };
    }

    public async Task<AiExtractReferenceResponseDto> ExtractReferenceAsync(
        AiExtractReferenceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(_extractReferenceEndpoint, new
        {
            videoUrl = request.VideoUrl,
            exerciseType = request.ExerciseType
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException($"AI reference extraction failed: {(int)response.StatusCode} {errorBody}");
        }

        var payload = await response.Content.ReadFromJsonAsync<AiExtractReferenceResponseContract>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new BadRequestException("AI service returned an empty reference extraction response.");
        }

        return new AiExtractReferenceResponseDto
        {
            RepetitionCount = payload.RepetitionCount,
            JointAngles = payload.JointAngles.ValueKind == JsonValueKind.Undefined
                ? "{}"
                : payload.JointAngles.GetRawText()
        };
    }

    public async Task<AiAnalysisResponseDto> CompareAsync(
        AiCompareRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(_compareEndpoint, new
        {
            childVideoUrl = request.ChildVideoUrl,
            referenceJointAnglesJson = request.ReferenceJointAnglesJson,
            referenceRepetitionCount = request.ReferenceRepetitionCount
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException($"AI compare failed: {(int)response.StatusCode} {errorBody}");
        }

        var payload = await response.Content.ReadFromJsonAsync<AiResponseContract>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new BadRequestException("AI service returned an empty compare response.");
        }

        return new AiAnalysisResponseDto
        {
            AccuracyScore = payload.AccuracyScore,
            RepetitionCount = payload.RepetitionCount,
            MistakeCount = payload.MistakeCount,
            Feedback = payload.Feedback ?? string.Empty,
            JointAngles = payload.JointAngles.ValueKind == JsonValueKind.Undefined
                ? "{}"
                : payload.JointAngles.GetRawText()
        };
    }

    private sealed class AiResponseContract
    {
        public decimal AccuracyScore { get; set; }
        public int RepetitionCount { get; set; }
        public int MistakeCount { get; set; }
        public string? Feedback { get; set; }
        public JsonElement JointAngles { get; set; }
    }

    private sealed class AiExtractReferenceResponseContract
    {
        public int RepetitionCount { get; set; }
        public JsonElement JointAngles { get; set; }
    }
}
