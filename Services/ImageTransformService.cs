using Avalonia;

namespace DicomViewer.Services;

public class ImageTransformService : IImageTransformService
{
    public Matrix CalculateZoomTransform(Point center, double zoomFactor, Matrix currentTransform)
    {
        // 마우스 위치를 중심으로 줌
        var matrix = Matrix.CreateTranslation(-center.X, -center.Y) *
                     Matrix.CreateScale(zoomFactor, zoomFactor) *
                     Matrix.CreateTranslation(center.X, center.Y) *
                     currentTransform;

        return matrix;
    }

    public Matrix CalculatePanTransform(Vector delta, Matrix currentTransform)
    {
        // 단순 이동
        return Matrix.CreateTranslation(delta.X, delta.Y) * currentTransform;
    }

    public Matrix CalculateFitToWindow(Size imageSize, Size windowSize)
    {
        if (imageSize.Width == 0 || imageSize.Height == 0)
            return Matrix.Identity;

        // 창 크기에 맞도록 스케일 계산
        var scaleX = windowSize.Width / imageSize.Width;
        var scaleY = windowSize.Height / imageSize.Height;
        var scale = Math.Min(scaleX, scaleY);

        return Matrix.CreateScale(scale, scale);
    }

    public Matrix ResetTransform()
    {
        return Matrix.Identity;
    }
}
