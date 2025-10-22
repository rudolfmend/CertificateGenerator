using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class SelectEventTopicWindow : Window
    {
        private readonly EventTopicRepository _repository;
        private List<EventTopicModel> _allEventTopics;
        public EventTopicModel SelectedEventTopic { get; private set; }

        public SelectEventTopicWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _repository = new EventTopicRepository(dbManager);
            LoadEventTopics();
        }

        private void LoadEventTopics()
        {
            try
            {
                _allEventTopics = _repository.GetAll();
                DgEventTopics.ItemsSource = _allEventTopics;

                if (_allEventTopics.Count == 0)
                {
                    MessageBox.Show("Nemáte žiadne uložené témy podujatí.\n\n" +
                                  "Pridajte tému pomocou kontextového menu pri poli 'Téma podujatia' v hlavnom okne.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní tém:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchTerm = TxtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                DgEventTopics.ItemsSource = _allEventTopics;
            }
            else
            {
                var filtered = _allEventTopics.Where(t =>
                    t.Topic.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Description != null &&
                     t.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                DgEventTopics.ItemsSource = filtered;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (DgEventTopics.SelectedItem is EventTopicModel selected)
            {
                SelectedEventTopic = selected;

                // Increment usage count
                _repository.IncrementUsage(selected.Id);

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Prosím vyberte tému zo zoznamu.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DgEventTopics.SelectedItem is EventTopicModel selected)
            {
                var result = MessageBox.Show(
                    $"Naozaj chcete odstrániť tému '{selected.Topic}'?\n\n" +
                    "Táto akcia je nevratná.",
                    "Potvrdenie odstránenia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repository.Delete(selected.Id);
                        LoadEventTopics();
                        MessageBox.Show("Téma bola úspešne odstránená.",
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
                MessageBox.Show("Prosím vyberte tému na odstránenie.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DgEventTopics_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgEventTopics.SelectedItem != null)
            {
                Select_Click(sender, e);
            }
        }
    }
}
