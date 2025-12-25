namespace DicomViewer.Models;

public class SeriesInfo
{
    public string SeriesInstanceUID { get; init; } = string.Empty;
    public int SeriesNumber { get; init; }
    public string SeriesDescription { get; init; } = string.Empty;
    public List<DicomImageModel> Images { get; init; } = new();
    public int CurrentImageIndex { get; set; } = 0;
}
