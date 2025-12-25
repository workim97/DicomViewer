using Avalonia.Controls;
using Avalonia.Input;
using DicomViewer.ViewModels;

namespace DicomViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 마우스 이벤트 연결
        var imageControl = this.FindControl<Image>("DicomImage");
        if (imageControl != null)
        {
            imageControl.PointerPressed += OnImagePointerPressed;
            imageControl.PointerMoved += OnImagePointerMoved;
            imageControl.PointerReleased += OnImagePointerReleased;
            imageControl.PointerWheelChanged += OnImagePointerWheelChanged;
        }

        // 키보드 이벤트
        this.KeyDown += OnKeyDown;
    }

    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnImagePointerPressed(sender, e);
        }
    }

    private void OnImagePointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnImagePointerMoved(sender, e);
        }
    }

    private void OnImagePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnImagePointerReleased(sender, e);
        }
    }

    private void OnImagePointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnImagePointerWheelChanged(sender, e);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            switch (e.Key)
            {
                case Key.Left:
                    vm.PreviousSliceCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Right:
                    vm.NextSliceCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.R:
                    vm.ResetViewCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
