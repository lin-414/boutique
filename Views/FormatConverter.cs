using System.Globalization;
using System.Windows.Data;

namespace Boutique.Views;

/// <summary>
///   Formats localized composite-format strings with one or more bound values.
///   values[0] is the localized format string (e.g. via lex:Loc), values[1..] are the arguments.
///   When values[1] is null or empty and an optional values[2] is supplied, that fallback text
///   is returned instead, preserving "empty state" behavior of StringFormat bindings.
/// </summary>
public class FormatConverter : IMultiValueConverter
{
  public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
  {
    if (values == null || values.Length == 0 || values[0] is not string format)
    {
      return string.Empty;
    }

    if (values.Length < 2 || values[1] is null or string { Length: 0 })
    {
      return values.Length > 2 && values[2] is string fallback ? fallback : string.Empty;
    }

    var args = new object[values.Length - 1];
    for (var i = 1; i < values.Length; i++)
    {
      args[i - 1] = values[i] ?? string.Empty;
    }

    try
    {
      return string.Format(culture, format, args);
    }
    catch (FormatException)
    {
      return format;
    }
  }

  public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}
