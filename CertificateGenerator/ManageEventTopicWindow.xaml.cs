using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    /// <summary>
    /// Unifikované okno pre správu tém podujatí - výber, pridávanie, úpravy a mazanie
    /// </summary>
    public partial class ManageEventTopicWindow : Window
    {
        private readonly EventTopicRepository _repository;
        private List<Data.EventTopicModel> _allTopics;
        private int _editingId = 0;

        public Data.EventTopicModel SelectedEventTopic { get; private set; }
        public bool IsSelectionMode { get; private set; }

        public ManageEventTopicWindow(DatabaseManager dbManager, bool selectionMode = true)
        {
            InitializeComponent();
            _repository = new EventTopicRepository(dbManager);
            IsSelectionMode = selectionMode;

            if (!IsSelectionMode)
            {
                BtnSelect.Visibility = Visibility.Collapsed;
            }

            LoadTopics();
        }

        private void LoadTopics()
        {
            try
            {
                _allTopics = _repository.GetAll();
                DgTopics.ItemsSource = _allTopics;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri načítaní tém:\n{ex.Message}",
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
                DgTopics.ItemsSource = _allTopics;
            }
            else
            {
                var filtered = _allTopics.Where(t =>
                    t.Topic.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Description != null &&
                     t.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                DgTopics.ItemsSource = filtered;
            }
        }

        private void DgTopics_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DgTopics.SelectedItem is Data.EventTopicModel selected)
            {
                LoadTopicToForm(selected);
            }
        }

        private void LoadTopicToForm(Data.EventTopicModel topic)
        {
            _editingId = topic.Id;
            TxtTopicName.Text = topic.Topic;
            TxtDescription.Text = topic.Description;
            TxtFormTitle.Text = "Upraviť tému";
            BtnSave.Content = "💾 Uložiť zmeny";
            TxtEditMode.Visibility = Visibility.Visible;
        }

        private void SaveTopic_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTopicName.Text))
            {
                MessageBox.Show(
                    "Názov témy je povinný.",
                    "Chýbajúce údaje",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                TxtTopicName.Focus();
                return;
            }

            try
            {
                var topic = new Data.EventTopicModel
                {
                    Id = _editingId,
                    Topic = TxtTopicName.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(TxtDescription.Text)
                        ? null
                        : TxtDescription.Text.Trim()
                };

                if (_editingId == 0)
                {
                    int newId = _repository.Add(topic);
                    MessageBox.Show(
                        "Téma bola úspešne pridaná.",
                        "Úspech",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    _repository.Update(topic);
                    MessageBox.Show(
                        "Téma bola úspešne aktualizovaná.",
                        "Úspech",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                LoadTopics();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri ukladaní témy:\n{ex.Message}",
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
            TxtTopicName.Clear();
            TxtDescription.Clear();
            TxtFormTitle.Text = "N téma";
            BtnSave.Content = "💾 Uložiť tému";
            TxtEditMode.Visibility = Visibility.Collapsed;
            DgTopics.SelectedItem = null;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSelectionMode)
            {
                return;
            }

            if (DgTopics.SelectedItem is Data.EventTopicModel selected)
            {
                SelectedEventTopic = selected;
                _repository.IncrementUsage(selected.Id);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Prosím vyberte tému zo zoznamu.",
                    "Informácia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DgTopics.SelectedItem is Data.EventTopicModel selected)
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
                        LoadTopics();
                        ClearForm();
                        MessageBox.Show(
                            "Téma bola úspešne odstránená.",
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
                    "Prosím vyberte tému na odstránenie.",
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

        private void DgTopics_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectionMode && DgTopics.SelectedItem != null)
            {
                Select_Click(sender, e);
            }
        }
    }
}
