using Microsoft.UI.Xaml;
using Windows.Storage;

namespace myshop.Helpers;

public static class ThemeHelper
{
    private const string ThemeKey = "AppTheme";
    private const string DefaultTheme = "System";

    /// <summary>
    /// Áp dụng theme cho ứng dụng
    /// </summary>
    public static void ApplyTheme(ElementTheme theme)
    {
        if (Application.Current?.Resources is not null)
        {
            Application.Current.RequestedTheme = theme switch
            {
                ElementTheme.Light => ApplicationTheme.Light,
                ElementTheme.Dark => ApplicationTheme.Dark,
                _ => Application.Current.RequestedTheme
            };
        }
    }

    /// <summary>
    /// Lưu theme preference
    /// </summary>
    public static void SaveTheme(string theme)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values[ThemeKey] = theme;
    }

    /// <summary>
    /// Lấy theme đã lưu
    /// </summary>
    public static string GetSavedTheme()
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values.TryGetValue(ThemeKey, out var value) && value is string theme)
        {
            return theme;
        }
        return DefaultTheme;
    }

    /// <summary>
    /// Chuyển đổi string theme sang ElementTheme
    /// </summary>
    public static ElementTheme ParseTheme(string theme)
    {
        return theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }
}

