namespace Rafiq.Application.DTOs.ProgressReports;

public class GenerateProgressReportRequestDto
{
    public int ChildId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
}
