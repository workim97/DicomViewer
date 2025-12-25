using Avalonia;

namespace DicomViewer.Services;

public interface IImageTransformService
{
    Matrix CalculateZoomTransform(Point center, double zoomFactor, Matrix currentTransform);
    Matrix CalculatePanTransform(Vector delta, Matrix currentTransform);
    Matrix CalculateFitToWindow(Size imageSize, Size windowSize);
    Matrix ResetTransform();
}
