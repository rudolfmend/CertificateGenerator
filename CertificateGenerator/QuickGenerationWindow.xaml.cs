using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
using iText.Kernel.Geom;
using Microsoft.Win32;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CertificateGenerator
{
    public partial class QuickGenerationWindow : Window
    {
        private ParticipantRepository _participantRepo;
        private OrganizerRepository _organizerRepo;
        private EventTopicRepository _topicRepo;
        private CertificateRepository _certificateRepo;
        private CertificateTemplateRepository _templateRepo;

        private int _currentOrganizerId = 0;
        private int _currentParticipantId = 0;
        private int _currentTopicId = 0;

        // Aktuálne vybraná šablóna
        private CertificateTemplateModel _currentTemplate;

        public QuickGenerationWindow()
        {
            InitializeComponent();
            InitializeRepositories();
            LoadDefaultTemplate();
        }

        private void InitializeRepositories()
        {
            try
            {
                var dbManager = App.DatabaseManager;
                _participantRepo = new ParticipantRepository(dbManager);
                _organizerRepo = new OrganizerRepository(dbManager);
                _topicRepo = new EventTopicRepository(dbManager);
                _certificateRepo = new CertificateRepository(dbManager);
                _templateRepo = new CertificateTemplateRepository(dbManager);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri inicializácii:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// N Načíta predvolenú šablónu alebo vytvorí základnú
        /// </summary>
        private void LoadDefaultTemplate()
        {
            try
            {
                _currentTemplate = _templateRepo.GetDefault();

                if (_currentTemplate != null)
                {
                    TxtSelectedTemplateName.Text = _currentTemplate.Name;
                }
                else
                {
                    // Ak neexistuje žiadna predvolená šablóna, použi minimalistickú modrú
                    var preset = ModernTemplatePresets.GetMinimalistBluePreset();
                    _currentTemplate = preset.Template;
                    TxtSelectedTemplateName.Text = "Minimalistická modrá (predvolená)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní predvolenej šablóny:\n{ex.Message}",
                    "Varovanie", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Fallback - použiť základnú šablónu
                _currentTemplate = new CertificateTemplateModel
                {
                    Name = "Základná šablóna"
                };
                TxtSelectedTemplateName.Text = "Základná šablóna";
            }
        }

        /// <summary>
        /// Otvorí galériu šablón
        /// </summary>
        private void ChangeTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var galleryWindow = new TemplateGalleryWindow(App.DatabaseManager);
                if (galleryWindow.ShowDialog() == true && galleryWindow.TemplateSelected)
                {
                    _currentTemplate = galleryWindow.SelectedTemplate;
                    TxtSelectedTemplateName.Text = _currentTemplate.Name;

                    Debug.WriteLine($"Šablóna '{_currentTemplate.Name}' bola úspešne vybraná.");
                    ToastHelper.Show(this, $"Šablóna '{_currentTemplate.Name}' bola úspešne vybraná.");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(
                //    $"Chyba pri otváraní galérie šablón:\n{ex.Message}",
                //    "Chyba",
                //    MessageBoxButton.OK,
                //    MessageBoxImage.Error);
                Debug.WriteLine($"Chyba pri otváraní galérie šablón:\n{ex.Message}");
                ToastHelper.Show(this, $"Chyba pri otváraní galérie šablón:\n{ex.Message}");
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void SelectOrganizer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new ManageOrganizerWindow(App.DatabaseManager);
                if (selectWindow.ShowDialog() == true)
                {
                    var selected = selectWindow.SelectedOrganizer;
                    TxtOrganizer.Text = selected.Name;
                    _currentOrganizerId = selected.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectEventTopic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new ManageEventTopicWindow(App.DatabaseManager);
                if (selectWindow.ShowDialog() == true)
                {
                    var selected = selectWindow.SelectedEventTopic;
                    TxtEventTopic.Text = selected.Topic;
                    _currentTopicId = selected.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectParticipant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new ParticipantManagerWindow(App.DatabaseManager);
                if (selectWindow.ShowDialog() == true)
                {
                    var selected = selectWindow.SelectedParticipant;
                    TxtParticipantName.Text = selected.Name;
                    _currentParticipantId = selected.Id;

                    if (selected.BirthDate.HasValue)
                        DtpBirthDate.SelectedDate = selected.BirthDate.Value;

                    TxtRegistrationNumber.Text = selected.RegistrationNumber;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// UPRAVENÁ FUNKČNOSŤ - Generuje PDF s vybranou šablónou
        /// </summary>
        private void GeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtParticipantName.Text))
            {
                ShowToast("Meno účastníka je povinné!", "warning");
                return;
            }

            // Skontroluj, či je šablóna načítaná
            if (_currentTemplate == null)
            {
                MessageBox.Show("Šablóna nie je načítaná. Načíta sa predvolená šablóna.",
                    "Varovanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadDefaultTemplate();

                if (_currentTemplate == null)
                {
                    MessageBox.Show("Nepodarilo sa načítať žiadnu šablónu!",
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF súbor (*.pdf)|*.pdf",
                    FileName = $"Certificate_{TxtParticipantName.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    PageSize pageSize = GetSelectedPageSize();

                    // Generuj PDF s vybranou šablónou
                    CertificatePdfGenerator.GeneratePdf(
                        saveDialog.FileName,
                        _currentTemplate,
                        TxtOrganizer.Text,
                        TxtEventTopic.Text,
                        DtpEventDate.SelectedDate,
                        TxtParticipantName.Text,
                        DtpBirthDate.SelectedDate,
                        TxtRegistrationNumber.Text,
                        TxtNotes.Text,
                        pageSize
                    );

                    // Ulož do histórie
                    SaveToHistory(saveDialog.FileName);

                    var result = MessageBox.Show(
                        $"Certifikát bol úspešne vytvorený!\n\nOtvoriť súbor?",
                        "Úspech",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    Debug.WriteLine($"Certifikát bol úspešne vytvorený! Otvoriť súbor?");
                    ToastHelper.Show(this, $"Certifikát bol úspešne vytvorený! Otvoriť súbor?");

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Chyba pri generovaní PDF:\n{ex.Message}",
                //    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Chyba pri generovaní PDF:\n{ex.Message}");
                ToastHelper.Show(this, $"Chyba pri generovaní PDF:\n{ex.Message}");
            }
        }

        private PageSize GetSelectedPageSize()
        {
            bool landscape = ChkLandscape.IsChecked == true;
            PageSize size;
            Debug.WriteLine($"private PageSize GetSelectedPageSize()");
            switch (CmbPaperFormat.SelectedIndex)
            {
                case 0: // A3
                    size = landscape ? PageSize.A3.Rotate() : PageSize.A3;
                    break;
                case 1: // A4
                    size = landscape ? PageSize.A4.Rotate() : PageSize.A4;
                    break;
                case 2: // A5
                default:
                    size = landscape ? PageSize.A5.Rotate() : PageSize.A5;
                    break;
            }

            return size;
        }

        private void SaveToHistory(string filePath)
        {
            try
            {
                // CertificateModel očakáva int, nie int?
                //   0 ako "žiadna hodnota" namiesto null
                var certificate = new CertificateModel
                {
                    OrganizerId = _currentOrganizerId,  // Priamo int (0 ak nie je nastavené)
                    OrganizerName = TxtOrganizer.Text,
                    EventTopicId = _currentTopicId,      // Priamo int (0 ak nie je nastavené)
                    EventTopicName = TxtEventTopic.Text,
                    EventDate = DtpEventDate.SelectedDate,
                    ParticipantId = _currentParticipantId, // Priamo int (0 ak nie je nastavené)
                    ParticipantName = TxtParticipantName.Text,
                    ParticipantBirthDate = DtpBirthDate.SelectedDate,
                    ParticipantRegistrationNumber = TxtRegistrationNumber.Text,
                    Notes = TxtNotes.Text,
                    FilePath = filePath,
                    PaperFormat = CmbPaperFormat.Text,
                    CreatedAt = DateTime.Now
                };
                Debug.WriteLine($"private void SaveToHistory()");
                _certificateRepo.Add(certificate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Certifikát bol vytvorený, ale nepodarilo sa uložiť históriu:\n{ex.Message}",
                    "Varovanie", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            TxtOrganizer.Clear();
            TxtEventTopic.Clear();
            DtpEventDate.SelectedDate = null;
            TxtParticipantName.Clear();
            DtpBirthDate.SelectedDate = null;
            TxtRegistrationNumber.Clear();
            TxtNotes.Clear();
            _currentOrganizerId = 0;
            _currentParticipantId = 0;
            _currentTopicId = 0;
        }

        private void OpenBulkGenerationWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                var bulkWindow = new BulkGenerationWindow();
                bulkWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Chyba pri otváraní okna hromadného generovania:\n{ex.Message}",
                //    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Chyba pri otváraní okna hromadného generovania:\n{ex.Message}");
                ToastHelper.Show(this, $"Chyba pri otváraní okna hromadného generovania:\n{ex.Message}");
            }
        }

        private void OpenTemplateEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editorWindow = new TemplateEditorWindow();
                editorWindow.ShowDialog();

                // Po zatvorení editora, aktualizuj šablónu ak bola zmenená
                LoadDefaultTemplate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní editora šablón:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    

        /// <summary>
        /// Zobrazí modernú toast notifikáciu na 5 sekúnd
        /// </summary>
        private async void ShowToast(string message, string type = "warning")
        {
            if (ToastNotification == null) return;

            // Nastavenie ikon a farieb podľa typu
            switch (type.ToLower())
            {
                case "error":
                    ToastIcon.Text = "❌";
                    ToastNotification.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    break;
                case "success":
                    ToastIcon.Text = "✅";
                    ToastNotification.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    break;
                case "info":
                    ToastIcon.Text = "ℹ️";
                    ToastNotification.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    break;
                default: // warning
                    ToastIcon.Text = "⚠️";
                    ToastNotification.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                    break;
            }

            ToastMessage.Text = message;
            ToastNotification.Visibility = Visibility.Visible;

            // Automatické skrytie po 5 sekundách
            await System.Threading.Tasks.Task.Delay(5000);
            if (ToastNotification != null)
            {
                ToastNotification.Visibility = Visibility.Collapsed;
            }
        }
    }
}
