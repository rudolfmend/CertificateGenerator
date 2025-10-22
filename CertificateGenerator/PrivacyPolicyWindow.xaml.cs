using System;
using System.Windows;

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
