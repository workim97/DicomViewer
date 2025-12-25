namespace DicomViewer.Models;

public class DicomImageModel
{
    public required byte[] PixelData { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int BitsAllocated { get; init; }
    public required int BitsStored { get; init; }
    public required string PhotometricInterpretation { get; init; }
    public double RescaleSlope { get; init; } = 1.0;
    public double RescaleIntercept { get; init; } = 0.0;
    public double WindowCenter { get; init; }
    public double WindowWidth { get; init; }
    public required string SOPInstanceUID { get; init; }
}
