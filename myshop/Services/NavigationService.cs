using Microsoft.UI.Xaml.Controls;
using System;

namespace myshop.Services;

public class NavigationService
{
    private Frame? _frame;
    private const string LastPageKey = "LastPageKey";

    /// <summary>
    /// Đăng ký Frame để navigation
    /// </summary>
    public void Initialize(Frame frame)
    {
        _frame = frame;
    }

    /// <summary>
    /// Navigate đến một page
    /// </summary>
    public bool NavigateTo(Type pageType, object? parameter = null)
    {
        if (_frame == null)
            return false;

        var result = _frame.Navigate(pageType, parameter);
        
        if (result)
        {
            // Lưu page cuối cùng
            SaveLastPage(pageType.Name);
        }
        
        return result;
    }

    /// <summary>
    /// Navigate về page trước
    /// </summary>
    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }

    /// <summary>
    /// Lưu page cuối cùng vào LocalSettings
    /// </summary>
    private void SaveLastPage(string pageName)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        localSettings.Values[LastPageKey] = pageName;
    }

    /// <summary>
    /// Lấy page cuối cùng đã lưu
    /// </summary>
    public string? GetLastPage()
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        if (localSettings.Values.TryGetValue(LastPageKey, out var value) && value is string pageName)
        {
            return pageName;
        }
        return null;
    }
}

