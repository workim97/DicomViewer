using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DicomViewer.Models;
using DicomViewer.Services;

namespace DicomViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDicomFileService _dicomFileService;
    private readonly IDicomImageService _dicomImageService;
    private readonly IImageTransformService _transformService;

    // 마우스 상호작용 상태
    private Point? _lastMousePosition;
    private bool _isWindowLevelDragging;
    private bool _isPanning;

    // Window reference for file dialogs
    private Window? _mainWindow;

    public MainWindowViewModel(
        IDicomFileService dicomFileService,
        IDicomImageService dicomImageService,
        IImageTransformService transformService)
    {
        _dicomFileService = dicomFileService;
        _dicomImageService = dicomImageService;
        _transformService = transformService;
    }

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    // Observable Properties
    [ObservableProperty]
    private WriteableBitmap? _currentImage;

    [ObservableProperty]
    private PatientInfo? _currentPatient;

    [ObservableProperty]
    private StudyInfo? _currentStudy;

    [ObservableProperty]
    private SeriesInfo? _currentSeries;

    [ObservableProperty]
    private ObservableCollection<DicomTagInfo> _dicomTags = new();

    [ObservableProperty]
    private int _currentSliceIndex;

    [ObservableProperty]
    private int _totalSlices;

    [ObservableProperty]
    private double _windowCenter = 40;

    [ObservableProperty]
    private double _windowLevel = 400;

    [ObservableProperty]
    private double _zoomLevel = 1.0;

    [ObservableProperty]
    private Matrix _imageTransform = Matrix.Identity;

    [ObservableProperty]
    private string _statusMessage = "준비";

    // Commands
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        if (_mainWindow == null) return;

        try
        {
            var topLevel = TopLevel.GetTopLevel(_mainWindow);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "DICOM 파일 열기",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("DICOM 파일") { Patterns = new[] { "*.dcm", "*.dicom", "*.*" } }
                    }
                });

            if (files.Count == 0) return;

            StatusMessage = "DICOM 파일 로딩 중...";

            var filePath = files[0].Path.LocalPath;
            var dicomFile = await _dicomFileService.LoadDicomFileAsync(filePath);

            if (dicomFile == null)
            {
                StatusMessage = "DICOM 파일 로드 실패";
                return;
            }

            // 정보 추출
            CurrentPatient = _dicomFileService.ExtractPatientInfo(dicomFile);
            CurrentStudy = _dicomFileService.ExtractStudyInfo(dicomFile);

            // 이미지 추출
            var imageModel = _dicomImageService.ExtractImageData(dicomFile);
            WindowCenter = imageModel.WindowCenter;
            WindowLevel = imageModel.WindowWidth;

            // 단일 이미지로 시리즈 생성
            CurrentSeries = new SeriesInfo
            {
                Images = new List<DicomImageModel> { imageModel }
            };
            CurrentSliceIndex = 0;
            TotalSlices = 1;

            // 태그 추출
            var tags = _dicomFileService.ExtractAllTags(dicomFile);
            DicomTags = new ObservableCollection<DicomTagInfo>(tags);

            // 이미지 렌더링
            UpdateImage();

            StatusMessage = "DICOM 파일 로드 완료";
        }
        catch (Exception ex)
        {
            StatusMessage = $"오류: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task OpenSeriesAsync()
    {
        if (_mainWindow == null) return;

        try
        {
            var topLevel = TopLevel.GetTopLevel(_mainWindow);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "DICOM 시리즈 열기",
                    AllowMultiple = true,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("DICOM 파일") { Patterns = new[] { "*.dcm", "*.dicom", "*.*" } }
                    }
                });

            if (files.Count == 0) return;

            StatusMessage = $"DICOM 시리즈 로딩 중 ({files.Count}개 파일)...";

            var filePaths = files.Select(f => f.Path.LocalPath).ToArray();
            var dicomFiles = await _dicomFileService.LoadDicomSeriesAsync(filePaths);

            if (dicomFiles.Count == 0)
            {
                StatusMessage = "DICOM 파일 로드 실패";
                return;
            }

            // 첫 번째 파일에서 정보 추출
            var firstFile = dicomFiles[0];
            CurrentPatient = _dicomFileService.ExtractPatientInfo(firstFile);
            CurrentStudy = _dicomFileService.ExtractStudyInfo(firstFile);

            // 모든 이미지 추출
            var images = new List<DicomImageModel>();
            foreach (var dicomFile in dicomFiles)
            {
                var imageModel = _dicomImageService.ExtractImageData(dicomFile);
                images.Add(imageModel);
            }

            // 시리즈 생성
            var seriesInfo = _dicomFileService.ExtractSeriesInfo(dicomFiles);
            seriesInfo.Images.AddRange(images);
            CurrentSeries = seriesInfo;

            CurrentSliceIndex = 0;
            TotalSlices = images.Count;

            // 첫 번째 이미지의 Window/Level 사용
            var firstImage = images[0];
            WindowCenter = firstImage.WindowCenter;
            WindowLevel = firstImage.WindowWidth;

            // 태그 추출
            var tags = _dicomFileService.ExtractAllTags(firstFile);
            DicomTags = new ObservableCollection<DicomTagInfo>(tags);

            // 이미지 렌더링
            UpdateImage();

            StatusMessage = $"DICOM 시리즈 로드 완료 ({TotalSlices}개 슬라이스)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"오류: {ex.Message}";
        }
    }

    [RelayCommand]
    private void NextSlice()
    {
        if (CurrentSeries == null) return;

        if (CurrentSliceIndex < CurrentSeries.Images.Count - 1)
        {
            CurrentSliceIndex++;
            UpdateImage();
        }
    }

    [RelayCommand]
    private void PreviousSlice()
    {
        if (CurrentSeries == null) return;

        if (CurrentSliceIndex > 0)
        {
            CurrentSliceIndex--;
            UpdateImage();
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (CurrentImage == null) return;

        var centerPoint = new Point(CurrentImage.PixelSize.Width / 2.0, CurrentImage.PixelSize.Height / 2.0);
        ImageTransform = _transformService.CalculateZoomTransform(centerPoint, 1.2, ImageTransform);
        ZoomLevel *= 1.2;
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (CurrentImage == null) return;

        var centerPoint = new Point(CurrentImage.PixelSize.Width / 2.0, CurrentImage.PixelSize.Height / 2.0);
        ImageTransform = _transformService.CalculateZoomTransform(centerPoint, 0.8, ImageTransform);
        ZoomLevel *= 0.8;
    }

    [RelayCommand]
    private void ResetView()
    {
        ImageTransform = _transformService.ResetTransform();
        ZoomLevel = 1.0;
    }

    [RelayCommand]
    private void FitToWindow()
    {
        if (CurrentImage == null || _mainWindow == null) return;

        var imageSize = new Size(CurrentImage.PixelSize.Width, CurrentImage.PixelSize.Height);
        var windowSize = new Size(_mainWindow.ClientSize.Width - 250, _mainWindow.ClientSize.Height - 300); // 패널 크기 고려

        ImageTransform = _transformService.CalculateFitToWindow(imageSize, windowSize);
    }

    // Helper Methods
    private void UpdateImage()
    {
        if (CurrentSeries == null || CurrentSeries.Images.Count == 0)
            return;

        var imageModel = CurrentSeries.Images[CurrentSliceIndex];
        CurrentImage = _dicomImageService.RenderToAvaloniaBitmap(imageModel, WindowCenter, WindowLevel);
    }

    partial void OnWindowCenterChanged(double value)
    {
        UpdateImage();
    }

    partial void OnWindowLevelChanged(double value)
    {
        UpdateImage();
    }

    // Mouse Interaction Methods
    public void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint((Control)sender!);
        _lastMousePosition = point.Position;

        if (point.Properties.IsLeftButtonPressed)
        {
            _isWindowLevelDragging = true;
            e.Handled = true;
        }
        else if (point.Properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            e.Handled = true;
        }
    }

    public void OnImagePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_lastMousePosition == null) return;

        var currentPosition = e.GetCurrentPoint((Control)sender!).Position;
        var delta = currentPosition - _lastMousePosition.Value;

        if (_isWindowLevelDragging)
        {
            var (newCenter, newLevel) = _dicomImageService.CalculateWindowLevelFromMouseDelta(
                delta.X, delta.Y, WindowCenter, WindowLevel);
            WindowCenter = newCenter;
            WindowLevel = newLevel;
            e.Handled = true;
        }
        else if (_isPanning)
        {
            ImageTransform = _transformService.CalculatePanTransform(new Vector(delta.X, delta.Y), ImageTransform);
            e.Handled = true;
        }

        _lastMousePosition = currentPosition;
    }

    public void OnImagePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isWindowLevelDragging = false;
        _isPanning = false;
        _lastMousePosition = null;
    }

    public void OnImagePointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Ctrl + 휠: 줌
            if (CurrentImage == null) return;

            var delta = e.Delta.Y;
            var zoomFactor = delta > 0 ? 1.1 : 0.9;
            var point = e.GetCurrentPoint((Control)sender!).Position;

            ImageTransform = _transformService.CalculateZoomTransform(point, zoomFactor, ImageTransform);
            ZoomLevel *= zoomFactor;
            e.Handled = true;
        }
        else
        {
            // 일반 휠: 슬라이스 탐색
            if (e.Delta.Y > 0)
                PreviousSliceCommand.Execute(null);
            else if (e.Delta.Y < 0)
                NextSliceCommand.Execute(null);
            e.Handled = true;
        }
    }
}
