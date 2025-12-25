using Avalonia.Media.Imaging;
using DicomViewer.Models;
using FellowOakDicom;

namespace DicomViewer.Services;

public interface IDicomImageService
{
    DicomImageModel ExtractImageData(DicomFile dicomFile);
    WriteableBitmap RenderToAvaloniaBitmap(DicomImageModel imageModel, double windowCenter, double windowLevel);
    (double center, double level) CalculateWindowLevelFromMouseDelta(double deltaX, double deltaY, double currentCenter, double currentLevel);
}
