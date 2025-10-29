using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CertificateGenerator.Converters
{
    /// <summary>
    /// Konvertor pre konverziu Boolean hodnôt na Visibility
    /// Používa sa v XAML bindingoch pre podmienené zobrazenie UI elementov
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Konvertuje Boolean na Visibility
        /// </summary>
        /// <param name="value">Boolean hodnota</param>
        /// <param name="targetType">Cieľový typ (Visibility)</param>
        /// <param name="parameter">Voliteľný parameter - ak je "Inverted", invertuje logiku</param>
        /// <param name="culture">Kultúra</param>
        /// <returns>Visibility.Visible ak true, inak Visibility.Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Podpora invertovanej logiky
                bool isInverted = parameter?.ToString()?.ToLower() == "inverted";

                if (isInverted)
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Konvertuje Visibility späť na Boolean
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isInverted = parameter?.ToString()?.ToLower() == "inverted";

                if (isInverted)
                {
                    return visibility != Visibility.Visible;
                }

                return visibility == Visibility.Visible;
            }

            return false;
        }
    }
}
