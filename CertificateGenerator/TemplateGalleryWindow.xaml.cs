using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CertificateGenerator.Data;
using CertificateGenerator.Helpers;

namespace CertificateGenerator
{
    /// <summary>
    /// Galéria moderných šablón certifikátov
    /// </summary>
    public partial class TemplateGalleryWindow : Window
    {
        private List<TemplatePresetViewModel> _allTemplates;
        private TemplatePresetViewModel _selectedTemplate;
        private readonly DatabaseManager _dbManager;
        private readonly CertificateTemplateRepository _templateRepository;

        public CertificateTemplateModel SelectedTemplate { get; private set; }
        public bool TemplateSelected { get; private set; }

        public TemplateGalleryWindow(DatabaseManager dbManager)
        {
            InitializeComponent();
            _dbManager = dbManager;
            _templateRepository = new CertificateTemplateRepository(_dbManager);

            LoadTemplates();
        }

        /// <summary>
        /// Načíta všetky dostupné šablóny
        /// </summary>
        private void LoadTemplates()
        {
            var presets = ModernTemplatePresets.GetAllPresets();
            _allTemplates = presets.Select(p => new TemplatePresetViewModel(p)).ToList();

            TemplatesItemsControl.ItemsSource = _allTemplates;
        }

        /// <summary>
        /// Klik na šablónu
        /// </summary>
        private void Template_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is TemplatePresetViewModel template)
            {
                SelectTemplate(template);
            }
        }

        /// <summary>
        /// Vyberie šablónu
        /// </summary>
        private void SelectTemplate(TemplatePresetViewModel template)
        {
            // Zruš predchádzajúci výber
            if (_selectedTemplate != null)
            {
                _selectedTemplate.IsSelected = false;
            }

            // Vyber novú šablónu
            _selectedTemplate = template;
            _selectedTemplate.IsSelected = true;

            BtnUseTemplate.IsEnabled = true;
        }

        /// <summary>
        /// Filter kategórií
        /// </summary>
        private void CategoryFilter_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && _allTemplates != null)
            {
                string category = rb.Content?.ToString();

                if (category == "Všetky")
                {
                    TemplatesItemsControl.ItemsSource = _allTemplates;
                }
                else
                {
                    var filtered = _allTemplates.Where(t => t.Category == category).ToList();
                    TemplatesItemsControl.ItemsSource = filtered;
                }
            }
        }

        /// <summary>
        /// Použije vybranú šablónu
        /// </summary>
        private void UseSelectedTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTemplate == null)
            {
                MessageBox.Show(
                    "Prosím, vyberte šablónu.",
                    "Žiadna šablóna",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                // Skopíruj šablónu
                SelectedTemplate = _selectedTemplate.Preset.Template.Clone();
                TemplateSelected = true;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri použití šablóny:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Otvorí editor pre vytvorenie vlastnej šablóny
        /// </summary>
        private void CreateCustomTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // OPRAVENÉ: Bez parametra (existujúci konštruktor)
                var editorWindow = new TemplateEditorWindow();

                // Po zatvorení editora, môžeme načítať šablóny znova
                if (editorWindow.ShowDialog() == true)
                {
                    // Načítaj šablóny znova z databázy
                    var allTemplates = _templateRepository.GetAll();

                    if (allTemplates != null && allTemplates.Count > 0)
                    {
                        // Použi poslednú vytvorenú šablónu
                        SelectedTemplate = allTemplates[allTemplates.Count - 1];
                        TemplateSelected = true;
                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri otváraní editora šablón:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Escape na zatvorenie okna
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }

    /// <summary>
    /// ViewModel pre šablónu s podporou INotifyPropertyChanged
    /// </summary>
    public class TemplatePresetViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public TemplatePreset Preset { get; set; }

        public string Name => Preset.Name;
        public string Description => Preset.Description;
        public string Category => Preset.Category;
        public string PreviewImagePath => Preset.PreviewImagePath;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public TemplatePresetViewModel(TemplatePreset preset)
        {
            Preset = preset;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
