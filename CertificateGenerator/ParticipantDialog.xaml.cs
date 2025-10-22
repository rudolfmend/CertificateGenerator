using System;
using System.Windows;

namespace CertificateGenerator
{
    public partial class ParticipantDialog : Window
    {
        public Participant Participant { get; private set; }

        public ParticipantDialog()
        {
            InitializeComponent();
            Participant = new Participant();
        }

        public ParticipantDialog(Participant existingParticipant) : this()
        {
            Participant = existingParticipant;
            LoadParticipantData();
        }

        private void LoadParticipantData()
        {
            TxtName.Text = Participant.Name;
            DtpBirthDate.SelectedDate = Participant.BirthDate;
            TxtRegistrationNumber.Text = Participant.RegistrationNumber;
            TxtNotes.Text = Participant.Notes;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Prosím vyplňte meno účastníka.", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            Participant.Name = TxtName.Text.Trim();
            Participant.BirthDate = DtpBirthDate.SelectedDate;
            Participant.RegistrationNumber = TxtRegistrationNumber.Text?.Trim();
            Participant.Notes = TxtNotes.Text?.Trim();

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
