namespace DicomViewer.Models;

public class DicomTagInfo
{
    public required string Tag { get; init; }
    public required string VR { get; init; }
    public required string Value { get; init; }
}
