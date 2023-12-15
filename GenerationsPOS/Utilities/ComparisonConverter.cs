using Avalonia.Data;
using Avalonia.Data.Converters;
using System;

namespace GenerationsPOS.Utilities
{
    /// <summary>
    /// Class utilized in mapping of UI objects to enum: https://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum/2908885#2908885
    /// </summary>
    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
        }
    }
}
