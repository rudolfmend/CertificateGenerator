using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CertificateGenerator
{
    public partial class SaveNewTopicWindow : Window
    {

        public string TopicName { get; private set; }
        public string Description { get; private set; }

        public SaveNewTopicWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTopicName.Text))
            {
                MessageBox.Show("Prosím vyplňte názov témy.", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTopicName.Focus();
                return;
            }

            TopicName = TxtTopicName.Text.Trim();
            Description = TxtDescription.Text?.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
