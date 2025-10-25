using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class ParticipantManagerWindow : Window
    {
        private readonly ParticipantRepository _repository;
        private List<ParticipantModel> _allParticipants;
        public ParticipantModel SelectedParticipant { get; private set; }
        private int _currentEditingId = 0;

        public ParticipantManagerWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _repository = new ParticipantRepository(dbManager);
            LoadParticipants();
        }

        private void LoadParticipants()
        {
            _allParticipants = _repository.GetAll();
            DgParticipants.ItemsSource = _allParticipants;
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchTerm = TxtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
                DgParticipants.ItemsSource = _allParticipants;
            else
            {
                DgParticipants.ItemsSource = _allParticipants.Where(p =>
                    p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (p.RegistrationNumber != null &&
                     p.RegistrationNumber.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }
        }

        private void DgParticipants_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgParticipants.SelectedItem is ParticipantModel selected)
            {
                LoadParticipantToForm(selected);
            }
        }

        private void LoadParticipantToForm(ParticipantModel participant)
        {
            _currentEditingId = participant.Id;
            TxtName.Text = participant.Name;
            DtpBirthDate.SelectedDate = participant.BirthDate;
            TxtRegistrationNumber.Text = participant.RegistrationNumber;
            TxtNotes.Text = participant.Notes;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Vyplňte meno účastníka.", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var participant = new ParticipantModel
            {
                Id = _currentEditingId,
                Name = TxtName.Text.Trim(),
                BirthDate = DtpBirthDate.SelectedDate,
                RegistrationNumber = TxtRegistrationNumber.Text?.Trim(),
                Notes = TxtNotes.Text?.Trim()
            };

            try
            {
                if (_currentEditingId > 0)
                    _repository.Update(participant);
                else
                    _repository.Add(participant);

                MessageBox.Show("Účastník bol uložený.", "Úspech",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadParticipants();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DgParticipants.SelectedItem is ParticipantModel selected)
            {
                if (MessageBox.Show($"Odstrániť '{selected.Name}'?", "Potvrdenie",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _repository.Delete(selected.Id);
                    LoadParticipants();
                    ClearForm();
                }
            }
        }

        private void SelectAndClose_Click(object sender, RoutedEventArgs e)
        {
            if (DgParticipants.SelectedItem is ParticipantModel selected)
            {
                SelectedParticipant = selected;
                _repository.IncrementUsage(selected.Id);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Vyberte účastníka zo zoznamu.", "Informácia",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearForm()
        {
            _currentEditingId = 0;
            TxtName.Clear();
            DtpBirthDate.SelectedDate = null;
            TxtRegistrationNumber.Clear();
            TxtNotes.Clear();
        }
    }
}
