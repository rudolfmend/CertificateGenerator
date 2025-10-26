using System;
using System.Windows;
using System.Windows.Input;

namespace CertificateGenerator
{
    /// <summary>
    /// Interaction logic for PrivacyPolicyWindow.xaml
    /// </summary>
    public partial class PrivacyPolicyWindow : Window
    {
        public PrivacyPolicyWindow()
        {
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
