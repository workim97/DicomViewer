using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using DicomViewer.ViewModels;
using DicomViewer.Views;
using DicomViewer.Services;

namespace DicomViewer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // 서비스 생성
            var dicomFileService = new DicomFileService();
            var dicomImageService = new DicomImageService();
            var transformService = new ImageTransformService();

            // ViewModel 생성 및 서비스 주입
            var viewModel = new MainWindowViewModel(
                dicomFileService,
                dicomImageService,
                transformService);

            // MainWindow 생성 및 설정
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            // ViewModel에 Window 참조 전달 (파일 다이얼로그용)
            viewModel.SetMainWindow(mainWindow);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}