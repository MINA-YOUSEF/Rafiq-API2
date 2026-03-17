namespace Rafiq.Application.DTOs.MedicalReports;

public class CreateMedicalReportRequestDto
{
    public int ChildId { get; set; }
    public int MediaId { get; set; }
    public string? Notes { get; set; }
}
