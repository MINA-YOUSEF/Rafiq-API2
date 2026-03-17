namespace Rafiq.Application.DTOs.MedicalReports;

public class MedicalReportDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int UploadedByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
