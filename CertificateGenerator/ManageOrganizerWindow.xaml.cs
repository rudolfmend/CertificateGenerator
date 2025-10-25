using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    /// <summary>
    /// Unifikované okno pre správu organizátorov - výber, pridávanie, úpravy a mazanie
    /// </summary>
    public partial class ManageOrganizerWindow : Window
    {
        private readonly OrganizerRepository _repository;
        private List<Data.OrganizerModel> _allOrganizers;
        private int _editingId = 0; // 0 = nový, >0 = úprava existujúceho

        public Data.OrganizerModel SelectedOrganizer { get; private set; }
        public bool IsSelectionMode { get; private set; }

        /// <summary>
        /// Konštruktor pre režim výberu organizátora
        /// </summary>
        public ManageOrganizerWindow(DatabaseManager dbManager, bool selectionMode = true)
        {
            InitializeComponent();
            _repository = new OrganizerRepository(dbManager);
            IsSelectionMode = selectionMode;

            // Skryť tlačidlo "Vybrať" ak nie sme v režime výberu
            if (!IsSelectionMode)
            {
                BtnSelect.Visibility = Visibility.Collapsed;
            }

            LoadOrganizers();
        }

        private void LoadOrganizers()
        {
            try
            {
                _allOrganizers = _repository.GetAll();
                DgOrganizers.ItemsSource = _allOrganizers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri načítaní organizátorov:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

        private void DgOrganizers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DgOrganizers.SelectedItem is Data.OrganizerModel selected)
            {
                // Načítať údaje do formulára pre úpravu
                LoadOrganizerToForm(selected);
            }
        }

        private void LoadOrganizerToForm(Data.OrganizerModel organizer)
        {
            _editingId = organizer.Id;
            TxtOrganizerName.Text = organizer.Name;
            TxtDescription.Text = organizer.Description;
            TxtFormTitle.Text = "Upraviť organizátora";
            BtnSave.Content = "💾 Uložiť zmeny";
            TxtEditMode.Visibility = Visibility.Visible;
        }

        private void SaveOrganizer_Click(object sender, RoutedEventArgs e)
        {
            // Validácia
            if (string.IsNullOrWhiteSpace(TxtOrganizerName.Text))
            {
                MessageBox.Show(
                    "Názov organizátora je povinný.",
                    "Chýbajúce údaje",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                TxtOrganizerName.Focus();
                return;
            }

            try
            {
                var organizer = new Data.OrganizerModel
                {
                    Id = _editingId,
                    Name = TxtOrganizerName.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(TxtDescription.Text)
                        ? null
                        : TxtDescription.Text.Trim()
                };

                if (_editingId == 0)
                {
                    // Nový organizátor
                    int newId = _repository.Add(organizer);
                    MessageBox.Show(
                        "Organizátor bol úspešne pridaný.",
                        "Úspech",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // Úprava existujúceho
                    _repository.Update(organizer);
                    MessageBox.Show(
                        "Organizátor bol úspešne aktualizovaný.",
                        "Úspech",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                LoadOrganizers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri ukladaní organizátora:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _editingId = 0;
            TxtOrganizerName.Clear();
            TxtDescription.Clear();
            TxtFormTitle.Text = "Nový organizátor";
            BtnSave.Content = "💾 Uložiť organizátora";
            TxtEditMode.Visibility = Visibility.Collapsed;
            DgOrganizers.SelectedItem = null;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSelectionMode)
            {
                return;
            }

            if (DgOrganizers.SelectedItem is Data.OrganizerModel selected)
            {
                SelectedOrganizer = selected;

                // Zvýšiť počítadlo použitia
                _repository.IncrementUsage(selected.Id);

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Prosím vyberte organizátora zo zoznamu.",
                    "Informácia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DgOrganizers.SelectedItem is Data.OrganizerModel selected)
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
                        ClearForm();
                        MessageBox.Show(
                            "Organizátor bol úspešne odstránený.",
                            "Úspech",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Chyba pri odstraňovaní:\n{ex.Message}",
                            "Chyba",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(
                    "Prosím vyberte organizátora na odstránenie.",
                    "Informácia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DgOrganizers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectionMode && DgOrganizers.SelectedItem != null)
            {
                Select_Click(sender, e);
            }
        }
    }
}
