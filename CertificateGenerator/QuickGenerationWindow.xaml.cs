using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
using iText.Kernel.Geom;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public QuickGenerationWindow()
        {
            InitializeComponent();
            InitializeRepositories();
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
                    DtpBirthDate.SelectedDate = selected.BirthDate;
                    TxtRegistrationNumber.Text = selected.RegistrationNumber;
                    TxtNotes.Text = selected.Notes;
                    _currentParticipantId = selected.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba:\n{ex.Message}", "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
                    SaveCertificateToHistory(saveDialog.FileName);

                    var result = MessageBox.Show(
                        $"PDF certifikát bol úspešne vytvorený!\n\nChcete ho otvoriť?",
                        "Úspech", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri vytváraní PDF:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreatePdfDocument(string filePath)
        {
            PageSize pageSize = GetSelectedPageSize();
            CertificateTemplateModel template = _templateRepo.GetDefault();

            CertificatePdfGenerator.GeneratePdf(
                filePath,
                template,
                TxtOrganizer.Text,
                TxtEventTopic.Text,
                DtpEventDate.SelectedDate,
                TxtParticipantName.Text,
                DtpBirthDate.SelectedDate,
                TxtRegistrationNumber.Text,
                TxtNotes.Text,
                pageSize
            );
        }

        private void SaveCertificateToHistory(string filePath)
        {
            try
            {
                var certificate = new CertificateModel
                {
                    OrganizerId = _currentOrganizerId,
                    OrganizerName = TxtOrganizer.Text ?? "",
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

                if (_currentOrganizerId > 0)
                    _organizerRepo.IncrementUsage(_currentOrganizerId);
                if (_currentParticipantId > 0)
                    _participantRepo.IncrementUsage(_currentParticipantId);
                if (_currentTopicId > 0)
                    _topicRepo.IncrementUsage(_currentTopicId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save history: {ex.Message}");
            }
        }

        private PageSize GetSelectedPageSize()
        {
            var selectedItem = CmbPaperFormat.SelectedItem as ComboBoxItem;
            string format = selectedItem?.Content.ToString() ?? "A5";

            if (format.StartsWith("A3")) return PageSize.A3;
            if (format.StartsWith("A4")) return PageSize.A4;
            return PageSize.A5;
        }

        private string GetSelectedPaperFormat()
        {
            var selectedItem = CmbPaperFormat.SelectedItem as ComboBoxItem;
            string format = selectedItem?.Content.ToString() ?? "A5";

            if (format.StartsWith("A3")) return "A3";
            if (format.StartsWith("A4")) return "A4";
            return "A5";
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace(" ", "_");
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            TxtOrganizer.Clear();
            TxtEventTopic.Clear();
            TxtParticipantName.Clear();
            TxtRegistrationNumber.Clear();
            TxtNotes.Clear();
            DtpEventDate.SelectedDate = null;
            DtpBirthDate.SelectedDate = null;
            CmbPaperFormat.SelectedIndex = 2;

            _currentOrganizerId = 0;
            _currentParticipantId = 0;
            _currentTopicId = 0;
        }

        private void OpenBulkGenerationWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                var bulkWindow = new BulkGenerationWindow(
                    TxtOrganizer.Text ?? "",
                    CmbPaperFormat.SelectedIndex
                );
                bulkWindow.Owner = this;
                bulkWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenTemplateEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editorWindow = new TemplateEditorWindow();
                editorWindow.Owner = this;
                editorWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
