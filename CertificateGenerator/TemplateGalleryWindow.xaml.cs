using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CertificateGenerator.Data;
using CertificateGenerator.Helpers;

namespace CertificateGenerator
{
    /// <summary>
    /// Typ správy pre zobrazenie v okne
    /// </summary>
    public enum MessageType
    {
        Info,
        Error,
        Success
    }

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
        /// Zobrazí správu v okne
        /// </summary>
        private void ShowMessage(string message, MessageType type = MessageType.Info)
        {
            MessageText.Text = message;

            // Nastavenie farby a ikony podľa typu správy
            switch (type)
            {
                case MessageType.Info:
                    MessageBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E40AF"));
                    MessageIcon.Text = "ℹ️";
                    break;
                case MessageType.Error:
                    MessageBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
                    MessageIcon.Text = "⚠️";
                    break;
                case MessageType.Success:
                    MessageBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"));
                    MessageIcon.Text = "✓";
                    break;
            }

            MessageBorder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Skryje správu
        /// </summary>
        private void HideMessage()
        {
            if (MessageBorder != null)
            {
                MessageBorder.Visibility = Visibility.Collapsed;
            }
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
            HideMessage(); // Skry predchádzajúce správy

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
            HideMessage(); // Skry predchádzajúce správy

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
                ShowMessage("Prosím, vyberte šablónu.", MessageType.Info);
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
                ShowMessage($"Chyba pri použití šablóny: {ex.Message}", MessageType.Error);
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
                ShowMessage($"Chyba pri otváraní editora šablón: {ex.Message}", MessageType.Error);
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
