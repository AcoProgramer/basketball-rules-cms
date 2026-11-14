using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using BasketballRulesCms.App.Services;

namespace BasketballRulesCms.App.Views.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibility : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c) => v is true ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => v is Visibility.Visible;
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class InverseBoolToVisibility : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c) => v is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => v is Visibility.Collapsed;
}

[ValueConversion(typeof(string), typeof(BitmapImage))]
public sealed class RelativePathToImage : IValueConverter
{
    public object? Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not string rel || string.IsNullOrWhiteSpace(rel)) return null;
        var abs = AppPaths.ToAbsolutePath(rel);
        if (!File.Exists(abs)) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource   = new Uri(abs, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

[ValueConversion(typeof(DateTimeOffset), typeof(string))]
public sealed class DateToString : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is DateTimeOffset dt
            ? dt.UtcDateTime.ToString("yyyy-MM-dd · HH:mm 'UTC'", CultureInfo.InvariantCulture)
            : string.Empty;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}
