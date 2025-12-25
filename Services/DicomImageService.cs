using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using DicomViewer.Models;

namespace DicomViewer.Services;

public class DicomImageService : IDicomImageService
{
    public DicomImageModel ExtractImageData(DicomFile dicomFile)
    {
        var dataset = dicomFile.Dataset;

        // 기본 이미지 정보 추출
        var width = dataset.GetSingleValue<int>(DicomTag.Columns);
        var height = dataset.GetSingleValue<int>(DicomTag.Rows);
        var bitsAllocated = dataset.GetSingleValue<int>(DicomTag.BitsAllocated);
        var bitsStored = dataset.GetSingleValue<int>(DicomTag.BitsStored);
        var photometricInterpretation = dataset.GetSingleValueOrDefault(DicomTag.PhotometricInterpretation, "MONOCHROME2");

        // Rescale 정보 추출
        var rescaleSlope = dataset.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1.0);
        var rescaleIntercept = dataset.GetSingleValueOrDefault(DicomTag.RescaleIntercept, 0.0);

        // Window/Level 정보 추출
        var windowCenter = dataset.GetSingleValueOrDefault(DicomTag.WindowCenter, 40.0);
        var windowWidth = dataset.GetSingleValueOrDefault(DicomTag.WindowWidth, 400.0);

        var sopInstanceUID = dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty);

        // 픽셀 데이터 추출
        var pixelData = DicomPixelData.Create(dataset);
        var frame = pixelData.GetFrame(0);

        // byte 배열로 변환
        var pixelDataBytes = frame.Data;

        return new DicomImageModel
        {
            PixelData = pixelDataBytes,
            Width = width,
            Height = height,
            BitsAllocated = bitsAllocated,
            BitsStored = bitsStored,
            PhotometricInterpretation = photometricInterpretation,
            RescaleSlope = rescaleSlope,
            RescaleIntercept = rescaleIntercept,
            WindowCenter = windowCenter,
            WindowWidth = windowWidth,
            SOPInstanceUID = sopInstanceUID
        };
    }

    public WriteableBitmap RenderToAvaloniaBitmap(DicomImageModel imageModel, double windowCenter, double windowLevel)
    {
        var width = imageModel.Width;
        var height = imageModel.Height;
        var pixelData = imageModel.PixelData;

        // WriteableBitmap 생성
        var bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        using (var buffer = bitmap.Lock())
        {
            unsafe
            {
                var ptr = (uint*)buffer.Address;

                // 픽셀 데이터를 Window/Level 적용하여 변환
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelIndex = y * width + x;

                        // 픽셀 값 추출 (16비트로 가정)
                        double pixelValue;
                        if (imageModel.BitsAllocated == 16)
                        {
                            ushort rawValue = BitConverter.ToUInt16(pixelData, pixelIndex * 2);
                            pixelValue = rawValue * imageModel.RescaleSlope + imageModel.RescaleIntercept;
                        }
                        else
                        {
                            pixelValue = pixelData[pixelIndex] * imageModel.RescaleSlope + imageModel.RescaleIntercept;
                        }

                        // Window/Level 알고리즘 적용
                        byte displayValue = ApplyWindowLevel(pixelValue, windowCenter, windowLevel);

                        // MONOCHROME1인 경우 반전
                        if (imageModel.PhotometricInterpretation == "MONOCHROME1")
                        {
                            displayValue = (byte)(255 - displayValue);
                        }

                        // BGRA 형식으로 변환 (그레이스케일이므로 R=G=B)
                        uint color = (uint)((255 << 24) | (displayValue << 16) | (displayValue << 8) | displayValue);
                        ptr[y * width + x] = color;
                    }
                }
            }
        }

        return bitmap;
    }

    private byte ApplyWindowLevel(double pixelValue, double windowCenter, double windowWidth)
    {
        double minValue = windowCenter - windowWidth / 2.0;
        double maxValue = windowCenter + windowWidth / 2.0;

        if (pixelValue <= minValue)
            return 0;
        if (pixelValue >= maxValue)
            return 255;

        // 선형 보간
        double normalized = (pixelValue - minValue) / windowWidth;
        return (byte)(normalized * 255.0);
    }

    public (double center, double level) CalculateWindowLevelFromMouseDelta(double deltaX, double deltaY, double currentCenter, double currentLevel)
    {
        // 민감도 설정
        const double centerSensitivity = 2.0;
        const double levelSensitivity = 2.0;

        var newCenter = currentCenter + (deltaX * centerSensitivity);
        var newLevel = currentLevel + (deltaY * levelSensitivity);

        // 범위 제한
        newCenter = Math.Clamp(newCenter, -1024, 3000);
        newLevel = Math.Clamp(newLevel, 1, 4000);

        return (newCenter, newLevel);
    }
}
