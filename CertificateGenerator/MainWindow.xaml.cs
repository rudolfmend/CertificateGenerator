using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
using iText.Kernel.Geom;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IOPath = System.IO.Path;

namespace CertificateGenerator
{
    public partial class MainWindow : Window
    {
        // Repositories
        private ParticipantRepository _participantRepo;
        private OrganizerRepository _organizerRepo;
        private EventTopicRepository _topicRepo;
        private CertificateRepository _certificateRepo;
        private CertificateTemplateRepository _templateRepo;

        // Current selected IDs (0 if not from database)
        private int _currentOrganizerId = 0;
        private int _currentParticipantId = 0;
        private int _currentTopicId = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeRepositories();
            InitializeComponent();
            InitializeRepositories();
        }

        //private void InitializeDefaults()
        //{
        //    // Set default paper format
        //    CmbPaperFormat.SelectedIndex = 2; // A5 as default
        //}

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
                MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== MAIN ACTION ==========

        //private void OpenBulkGeneration_Click(object sender, RoutedEventArgs e)
        //{
        //    var bulkWindow = new BulkGenerationWindow(
        //        TxtEventOrganizer.Text,
        //        CmbPaperFormat.SelectedIndex
        //    );
        //    bulkWindow.ShowDialog();
        //}

        private void OpenBulkGeneration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bulkWindow = new BulkGenerationWindow("", 2); // Empty organizer, A5 default
                bulkWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní generátora:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== SAVE METHODS ==========

        private void SaveOrganizer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEventOrganizer.Text))
            {
                MessageBox.Show("Prosím vyplňte názov organizátora.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var organizer = new OrganizerModel
            {
                Name = TxtEventOrganizer.Text.Trim()
            };

            try
            {
                int id = _organizerRepo.Add(organizer);
                _currentOrganizerId = id;

                MessageBox.Show("Organizátor bol úspešne uložený do databázy.",
                    "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri ukladaní organizátora:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveEventTopic_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEventTopic.Text))
            {
                MessageBox.Show("Prosím vyplňte tému podujatia.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var topic = new EventTopicModel
            {
                Topic = TxtEventTopic.Text.Trim()
            };

            try
            {
                int id = _topicRepo.Add(topic);
                _currentTopicId = id;

                MessageBox.Show("Téma podujatia bola úspešne uložená do databázy.",
                    "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri ukladaní témy:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtParticipantName.Text))
            {
                MessageBox.Show("Prosím vyplňte meno účastníka.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var participant = new ParticipantModel
            {
                Name = TxtParticipantName.Text.Trim(),
                BirthDate = DtpBirthDate.SelectedDate,
                RegistrationNumber = TxtRegistrationNumber.Text?.Trim(),
                Notes = TxtNotes.Text?.Trim()
            };

            try
            {
                int id = _participantRepo.Add(participant);
                _currentParticipantId = id;

                MessageBox.Show("Účastník bol úspešne uložený do databázy.",
                    "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri ukladaní účastníka:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== SELECT METHODS ==========

        private void BtnEventOrganizer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectOrganizerWindow(App.DatabaseManager);
                if (selectWindow.ShowDialog() == true)
                {
                    var selected = selectWindow.SelectedOrganizer;
                    TxtEventOrganizer.Text = selected.Name;
                    _currentOrganizerId = selected.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní okna:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEventTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectEventTopicWindow(App.DatabaseManager);
                if (selectWindow.ShowDialog() == true)
                {
                    var selected = selectWindow.SelectedEventTopic;
                    TxtEventTopic.Text = selected.Topic;
                    _currentTopicId = selected.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní okna:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnParticipants_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectParticipantWindow(App.DatabaseManager);
                if (selectWindow.ShowDialog() == true)
                {
                    var selected = selectWindow.SelectedParticipant;
                    TxtParticipantName.Text = selected.Name;
                    DtpBirthDate.SelectedDate = selected.BirthDate;
                    TxtRegistrationNumber.Text = selected.RegistrationNumber;
                    TxtNotes.Text = selected.Notes;
                    _currentParticipantId = selected.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní okna:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== PDF GENERATION ==========

        private void GeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtParticipantName.Text))
            {
                MessageBox.Show("Prosím vyplňte meno účastníka.", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "PDF súbory (*.pdf)|*.pdf",
                    FileName = $"Certificate_{SanitizeFileName(TxtParticipantName.Text)}.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    CreatePdfDocument(saveDialog.FileName);

                    // Save to certificate history
                    SaveCertificateToHistory(saveDialog.FileName);

                    MessageBox.Show($"PDF certifikát bol úspešne vytvorený!\n\n{saveDialog.FileName}",
                        "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri vytváraní PDF:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCertificateToHistory(string filePath)
        {
            try
            {
                var certificate = new CertificateModel
                {
                    OrganizerId = _currentOrganizerId,
                    OrganizerName = TxtEventOrganizer.Text ?? "",
                    ParticipantId = _currentParticipantId,
                    ParticipantName = TxtParticipantName.Text,
                    ParticipantBirthDate = DtpBirthDate.SelectedDate,
                    ParticipantRegistrationNumber = TxtRegistrationNumber.Text,
                    EventTopicId = _currentTopicId,
                    EventTopicName = TxtEventTopic.Text ?? "",
                    EventDate = DtpEventDate.SelectedDate,
                    Notes = TxtNotes.Text,
                    PaperFormat = GetSelectedPaperFormat(),
                    FilePath = filePath
                };

                _certificateRepo.Add(certificate);

                // Update usage counts
                if (_currentOrganizerId > 0)
                    _organizerRepo.IncrementUsage(_currentOrganizerId);
                if (_currentParticipantId > 0)
                    _participantRepo.IncrementUsage(_currentParticipantId);
                if (_currentTopicId > 0)
                    _topicRepo.IncrementUsage(_currentTopicId);
            }
            catch (Exception ex)
            {
                // Non-critical error, just log
                System.Diagnostics.Debug.WriteLine($"Failed to save certificate history: {ex.Message}");
            }
        }

        private string GetSelectedPaperFormat()
        {
            ComboBoxItem selectedItem = CmbPaperFormat.SelectedItem as ComboBoxItem;
            string format = selectedItem?.Content.ToString() ?? "A5";

            if (format.StartsWith("A3")) return "A3";
            if (format.StartsWith("A4")) return "A4";
            return "A5";
        }

        private void OpenBulkGenerationWindow()
        {
            var bulkWindow = new BulkGenerationWindow(
                TxtEventOrganizer.Text,
                CmbPaperFormat.SelectedIndex
            );
            bulkWindow.ShowDialog();
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in IOPath.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace(" ", "_");
        }

        private void CreatePdfDocument(string filePath)
        {
            PageSize pageSize = GetSelectedPageSize();

            // Načítame predvolenú šablónu
            CertificateTemplateModel template = _templateRepo.GetDefault();

            // Použijeme helper na generovanie PDF
            CertificatePdfGenerator.GeneratePdf(
                filePath,
                template,
                TxtEventOrganizer.Text,
                TxtEventTopic.Text,
                DtpEventDate.SelectedDate,
                TxtParticipantName.Text,
                DtpBirthDate.SelectedDate,
                TxtRegistrationNumber.Text,
                TxtNotes.Text,
                pageSize
            );
        }

        private PageSize GetSelectedPageSize()
        {
            ComboBoxItem selectedItem = CmbPaperFormat.SelectedItem as ComboBoxItem;
            string format = selectedItem?.Content.ToString() ?? "A5";

            switch (format)
            {
                case "A3":
                    return PageSize.A3;
                case "A4":
                    return PageSize.A4;
                case "A5":
                default:
                    return PageSize.A5;
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            TxtEventOrganizer.Clear();
            TxtEventTopic.Clear();
            TxtParticipantName.Clear();
            TxtRegistrationNumber.Clear();
            TxtNotes.Clear();
            DtpEventDate.SelectedDate = null;
            DtpBirthDate.SelectedDate = null;
            CmbPaperFormat.SelectedIndex = 2;

            // Reset IDs
            _currentOrganizerId = 0;
            _currentParticipantId = 0;
            _currentTopicId = 0;
        }

        #region MainWindow Clicking on buttons and borders

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void PrivacyPolicy_Click(object sender, RoutedEventArgs e)
        {
            var privacyWindow = new PrivacyPolicyWindow();
            privacyWindow.Owner = this;
            privacyWindow.ShowDialog();
        }



        private void OpenTemplateEditorBorder_Click(object sender, MouseButtonEventArgs e)
        {
            OpenTemplateEditor_Click(sender, e);
        }

        //private void OpenTemplateEditor_Click(object sender, MouseButtonEventArgs e)
        private void OpenTemplateEditor_Click(object sender, RoutedEventArgs e)
        {
            // Vytvoríme inštanciu nového okna
            TemplateEditorWindow editorWindow = new TemplateEditorWindow();

            // Skryjeme aktuálne okno (MainWindow)
            this.Hide();

            // Nastavíme správanie pri zatvorení editoru:
            // keď sa zavrie TemplateEditorWindow, ukáže sa späť MainWindow
            editorWindow.Closed += (s, args) =>
            {
                this.Show();
            };

            // Otvoríme nové okno
            editorWindow.Show();
        }

        //private void ManageData_Click(object sender, RoutedEventArgs e)
        //{
        //    // Otvoríme editor šablón
        //    var templateEditor = new TemplateEditorWindow();
        //    templateEditor.Owner = this;
        //    templateEditor.ShowDialog();
        //}
    

        // ========== MANAGEMENT WINDOWS ==========

        private void ManageParticipants_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectParticipantWindow(App.DatabaseManager);
                selectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní správy účastníkov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageOrganizers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectOrganizerWindow(App.DatabaseManager);
                selectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní správy organizátorov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageEventTopics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectEventTopicWindow(App.DatabaseManager);
                selectWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní správy tém:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CertificateHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Okno histórie certifikátov bude implementované v ďalšej verzii.",
                "Pripravované", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
