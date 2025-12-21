using Microsoft.UI.Xaml;

namespace myshop.Helpers;

public static class WindowHelper
{
    private static Window? _currentWindow;

    /// <summary>
    /// Đăng ký Window hiện tại
    /// </summary>
    public static void RegisterWindow(Window window)
    {
        _currentWindow = window;
    }

    /// <summary>
    /// Lấy Window hiện tại
    /// </summary>
    public static Window? GetCurrentWindow()
    {
        return _currentWindow;
    }

    /// <summary>
    /// Lấy Window theo type
    /// </summary>
    public static T? GetWindow<T>() where T : Window
    {
        return _currentWindow as T;
    }
}

