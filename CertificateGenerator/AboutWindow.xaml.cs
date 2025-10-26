using System;
using System.Windows;
using System.Windows.Input;

namespace CertificateGenerator
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadSystemInfo();
        }

        private void LoadSystemInfo()
        {
            try
            {
                // OS Version
                var os = Environment.OSVersion;
                TxtOSVersion.Text = $"{os.Platform} {os.Version}";

                // .NET Version
                TxtDotNetVersion.Text = $".NET Framework {Environment.Version}";

                // Architecture
                TxtArchitecture.Text = Environment.Is64BitProcess ? "x64" : "x86";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading system info: {ex.Message}");
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void LearnMorePrivacy_Click(object sender, RoutedEventArgs e)
        {
            var privacyWindow = new PrivacyPolicyWindow();
            privacyWindow.Owner = this;
            privacyWindow.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
