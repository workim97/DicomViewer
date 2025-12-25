namespace DicomViewer.Models;

public class StudyInfo
{
    public string StudyInstanceUID { get; init; } = string.Empty;
    public string StudyDate { get; init; } = string.Empty;
    public string StudyDescription { get; init; } = string.Empty;
    public string InstitutionName { get; init; } = string.Empty;
    public string Modality { get; init; } = string.Empty;
}
