namespace Rafiq.Application.Interfaces.Jobs;

public interface ISessionAnalysisJob
{
    Task ProcessAsync(int sessionId);
}
