using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CertificateGenerator.Helpers
{
    /// <summary>
    /// Helper trieda pre zobrazovanie toast notifikácií v celom projekte
    /// </summary>
    public static class ToastHelper
    {
        /// <summary>
        /// Zobrazí toast notifikáciu v danom okne na 5 sekúnd
        /// </summary>
        /// <param name="window">Window, v ktorom sa má toast zobraziť</param>
        /// <param name="message">Text správy</param>
        /// <param name="type">Typ: "warning", "error", "success", "info"</param>
        public static async void Show(Window window, string message, string type = "warning")
        {
            // Nájdi toast elementy v okne
            var toastBorder = window.FindName("ToastNotification") as Border;
            var toastIcon = window.FindName("ToastIcon") as TextBlock;
            var toastMessage = window.FindName("ToastMessage") as TextBlock;

            if (toastBorder == null || toastIcon == null || toastMessage == null)
            {
                // Fallback na MessageBox ak toast neexistuje
                MessageBox.Show(message, GetTitle(type), MessageBoxButton.OK, GetMessageBoxImage(type));
                return;
            }

            // Nastavenie ikon a farieb podľa typu
            switch (type.ToLower())
            {
                case "error":
                    toastIcon.Text = "❌";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    break;
                case "success":
                    toastIcon.Text = "✅";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    break;
                case "info":
                    toastIcon.Text = "ℹ️";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    break;
                default: // warning
                    toastIcon.Text = "⚠️";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                    break;
            }

            toastMessage.Text = message;
            toastBorder.Visibility = Visibility.Visible;

            // Automatické skrytie po 5 sekundách
            await System.Threading.Tasks.Task.Delay(5000);
            if (toastBorder != null)
            {
                toastBorder.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Zobrazí toast s možnosťou vlastného času zobrazenia
        /// </summary>
        public static async void Show(Window window, string message, string type, int durationMs)
        {
            var toastBorder = window.FindName("ToastNotification") as Border;
            var toastIcon = window.FindName("ToastIcon") as TextBlock;
            var toastMessage = window.FindName("ToastMessage") as TextBlock;

            if (toastBorder == null || toastIcon == null || toastMessage == null)
            {
                MessageBox.Show(message, GetTitle(type), MessageBoxButton.OK, GetMessageBoxImage(type));
                return;
            }

            switch (type.ToLower())
            {
                case "error":
                    toastIcon.Text = "❌";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    break;
                case "success":
                    toastIcon.Text = "✅";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    break;
                case "info":
                    toastIcon.Text = "ℹ️";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    break;
                default:
                    toastIcon.Text = "⚠️";
                    toastBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                    break;
            }

            toastMessage.Text = message;
            toastBorder.Visibility = Visibility.Visible;

            await System.Threading.Tasks.Task.Delay(durationMs);
            if (toastBorder != null)
            {
                toastBorder.Visibility = Visibility.Collapsed;
            }
        }

        private static string GetTitle(string type)
        {
            var lowerType = type != null ? type.ToLower() : string.Empty;
            if (lowerType == "error")
                return "Chyba";
            else if (lowerType == "success")
                return "Úspech";
            else if (lowerType == "info")
                return "Informácia";
            else
                return "Varovanie";
        }

        private static MessageBoxImage GetMessageBoxImage(string type)
        {
            var lowerType = type != null ? type.ToLower() : string.Empty;
            if (lowerType == "error")
                return MessageBoxImage.Error;
            else if (lowerType == "success" || lowerType == "info")
                return MessageBoxImage.Information;
            else
                return MessageBoxImage.Warning;
        }
    }
}
