using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class SelectMultipleParticipantsWindow : Window
    {
        private ObservableCollection<SelectableParticipant> _participants;
        public List<ParticipantModel> SelectedParticipants { get; private set; }

        public SelectMultipleParticipantsWindow(List<ParticipantModel> participants)
        {
            InitializeComponent();

            _participants = new ObservableCollection<SelectableParticipant>(
                participants.Select(p => new SelectableParticipant(p)));

            DgParticipants.ItemsSource = _participants;

            foreach (var p in _participants)
                p.PropertyChanged += Participant_PropertyChanged;
        }

        private void Participant_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
                UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            TxtSelectedCount.Text = _participants.Count(p => p.IsSelected).ToString();
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string search = TxtSearch.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(search))
            {
                foreach (var p in _participants)
                    p.IsVisible = true;
            }
            else
            {
                foreach (var p in _participants)
                {
                    p.IsVisible = p.Name.ToLower().Contains(search) ||
                                 (p.RegistrationNumber?.ToLower().Contains(search) ?? false);
                }
            }

            DgParticipants.Items.Refresh();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var p in _participants.Where(p => p.IsVisible))
                p.IsSelected = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            SelectedParticipants = _participants
                .Where(p => p.IsSelected)
                .Select(p => p.Participant)
                .ToList();

            if (SelectedParticipants.Count == 0)
            {
                MessageBox.Show("Prosím vyberte aspoň jedného účastníka.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class SelectableParticipant : INotifyPropertyChanged
    {
        public ParticipantModel Participant { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public string Name => Participant.Name;
        public DateTime? BirthDate => Participant.BirthDate;
        public string RegistrationNumber => Participant.RegistrationNumber;

        public SelectableParticipant(ParticipantModel participant)
        {
            Participant = participant;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
