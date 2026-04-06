using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace InternetBankCalculator.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public Brush? TrueBrush { get; set; }
    public Brush? FalseBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return (b ? TrueBrush : FalseBrush)!;
        }
        return FalseBrush!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}