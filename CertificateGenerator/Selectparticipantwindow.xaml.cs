using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class SelectParticipantWindow : Window
    {
        private readonly ParticipantRepository _repository;
        private List<ParticipantModel> _allParticipants;
        public ParticipantModel SelectedParticipant { get; private set; }

        public SelectParticipantWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _repository = new ParticipantRepository(dbManager);
            LoadParticipants();
        }

        private void LoadParticipants()
        {
            try
            {
                _allParticipants = _repository.GetAll();
                DgParticipants.ItemsSource = _allParticipants;

                if (_allParticipants.Count == 0)
                {
                    MessageBox.Show("Nemáte žiadnych uložených účastníkov.\n\n" +
                                  "Pridajte účastníkov pomocou tlačidla 'Uložiť ako nového účastníka' v hlavnom okne.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní účastníkov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchTerm = TxtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                DgParticipants.ItemsSource = _allParticipants;
            }
            else
            {
                var filtered = _allParticipants.Where(p =>
                    p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (p.RegistrationNumber != null &&
                     p.RegistrationNumber.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                DgParticipants.ItemsSource = filtered;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (DgParticipants.SelectedItem is ParticipantModel selected)
            {
                SelectedParticipant = selected;

                // Increment usage count
                _repository.IncrementUsage(selected.Id);

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Prosím vyberte účastníka zo zoznamu.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DgParticipants.SelectedItem is ParticipantModel selected)
            {
                var result = MessageBox.Show(
                    $"Naozaj chcete odstrániť účastníka '{selected.Name}'?\n\n" +
                    "Táto akcia je nevratná.",
                    "Potvrdenie odstránenia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repository.Delete(selected.Id);
                        LoadParticipants();
                        MessageBox.Show("Účastník bol úspešne odstránený.",
                            "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba pri odstraňovaní:\n{ex.Message}",
                            "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte účastníka na odstránenie.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DgParticipants_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgParticipants.SelectedItem != null)
            {
                Select_Click(sender, e);
            }
        }
    }
}
