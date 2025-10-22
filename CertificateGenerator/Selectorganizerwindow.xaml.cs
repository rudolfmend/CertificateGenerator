using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class SelectOrganizerWindow : Window
    {
        private readonly OrganizerRepository _repository;
        private List<OrganizerModel> _allOrganizers;
        public OrganizerModel SelectedOrganizer { get; private set; }

        public SelectOrganizerWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _repository = new OrganizerRepository(dbManager);
            LoadOrganizers();
        }

        private void LoadOrganizers()
        {
            try
            {
                _allOrganizers = _repository.GetAll();
                DgOrganizers.ItemsSource = _allOrganizers;

                if (_allOrganizers.Count == 0)
                {
                    MessageBox.Show("Nemáte žiadnych uložených organizátorov.\n\n" +
                                  "Pridajte organizátora pomocou kontextového menu pri poli 'Organizátor' v hlavnom okne.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní organizátorov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchTerm = TxtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                DgOrganizers.ItemsSource = _allOrganizers;
            }
            else
            {
                var filtered = _allOrganizers.Where(o =>
                    o.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (o.Description != null &&
                     o.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                DgOrganizers.ItemsSource = filtered;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (DgOrganizers.SelectedItem is OrganizerModel selected)
            {
                SelectedOrganizer = selected;

                // Increment usage count
                _repository.IncrementUsage(selected.Id);

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Prosím vyberte organizátora zo zoznamu.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DgOrganizers.SelectedItem is OrganizerModel selected)
            {
                var result = MessageBox.Show(
                    $"Naozaj chcete odstrániť organizátora '{selected.Name}'?\n\n" +
                    "Táto akcia je nevratná.",
                    "Potvrdenie odstránenia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repository.Delete(selected.Id);
                        LoadOrganizers();
                        MessageBox.Show("Organizátor bol úspešne odstránený.",
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
                MessageBox.Show("Prosím vyberte organizátora na odstránenie.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DgOrganizers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgOrganizers.SelectedItem != null)
            {
                Select_Click(sender, e);
            }
        }
    }
}
