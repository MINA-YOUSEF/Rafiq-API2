namespace Rafiq.Application.DTOs.ProgressReports;

public class ProgressReportDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int SpecialistProfileId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal ImprovementPercentage { get; set; }
    public string AccuracyTrendsJson { get; set; } = string.Empty;
    public int SessionFrequency { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
