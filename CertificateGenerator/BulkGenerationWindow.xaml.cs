using CertificateGenerator.Data;
using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IOPath = System.IO.Path;
using MessageBox = System.Windows.MessageBox;

namespace CertificateGenerator
{
    public partial class BulkGenerationWindow : Window
    {
        public ObservableCollection<Participant> Participants { get; set; }

        private CertificateTemplateRepository _templateRepo;

        public BulkGenerationWindow()
        {
            InitializeComponent();
            Participants = new ObservableCollection<Participant>();
            DgParticipants.ItemsSource = Participants;
            Participants.CollectionChanged += (s, e) => UpdateStatistics();
        }

        public BulkGenerationWindow(string organizer, int paperFormatIndex)
            : this()
        {
            TxtBulkOrganizer.Text = organizer;
            CmbBulkPaperFormat.SelectedIndex = paperFormatIndex;

            TxtBulkOrganizer.IsReadOnly = false;
            TxtBulkOrganizer.IsEnabled = true;
            _templateRepo = new CertificateTemplateRepository(App.DatabaseManager);
        }

        private void UpdateStatistics()
        {
            TxtParticipantCount.Text = Participants.Count.ToString();
            UpdatePdfCount();
        }

        private void TxtBulkTopics_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePdfCount();
        }

        private void UpdatePdfCount()
        {
            var topics = ParseTopics();
            int topicCount = topics.Count;
            int totalPdfs = Participants.Count * topicCount;

            TxtTopicsCount.Text = topicCount.ToString();
            TxtTopicsCountSummary.Text = topicCount.ToString();
            TxtPdfCount.Text = totalPdfs.ToString();
            TxtTotalPdfCount.Text = totalPdfs.ToString();
        }

        private List<string> ParseTopics()
        {
            if (string.IsNullOrWhiteSpace(TxtBulkTopics.Text))
                return new List<string>();

            var text = TxtBulkTopics.Text;
            var topics = new List<string>();

            // Split by newlines first
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Split each line by comma or semicolon
                var parts = line.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        topics.Add(trimmed);
                    }
                }
            }

            return topics;
        }

        private void AddParticipant_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ParticipantDialog();
            if (dialog.ShowDialog() == true)
            {
                Participants.Add(dialog.Participant);

                Debug.WriteLine($"Pridaný účastník: {dialog.Participant.Name}\n" +
                               $"Celkový počet: {Participants.Count}\n" +
                               $"DgParticipants.Items.Count: {DgParticipants.Items.Count}",
                               "DEBUG - Add Participant");

                MessageBox.Show($"Pridaný účastník: {dialog.Participant.Name}\n" +
                               $"Celkový počet: {Participants.Count}\n" +
                               $"DgParticipants.Items.Count: {DgParticipants.Items.Count}",
                               "DEBUG - Add Participant");
            }
        }

        private void EditParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (DgParticipants.SelectedItem is Participant selected)
            {
                var dialog = new ParticipantDialog(selected);
                if (dialog.ShowDialog() == true)
                {
                    var index = Participants.IndexOf(selected);
                    Participants[index] = dialog.Participant;
                    DgParticipants.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte účastníka na úpravu.", "Informácia",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (DgParticipants.SelectedItem is Participant selected)
            {
                var result = MessageBox.Show($"Naozaj chcete odstrániť účastníka '{selected.Name}'?",
                    "Potvrdenie", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Participants.Remove(selected);
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte účastníka na odstránenie.", "Informácia",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            if (Participants.Count > 0)
            {
                var result = MessageBox.Show("Naozaj chcete vymazať celý zoznam účastníkov?",
                    "Potvrdenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    Participants.Clear();
                }
            }
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV súbory (*.csv)|*.csv|Textové súbory (*.txt)|*.txt|Všetky súbory (*.*)|*.*",
                Title = "Importovať CSV so zoznamom účastníkov"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    ImportFromCsv(openDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba pri importovaní CSV:\n{ex.Message}",
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportFromCsv(string filePath)
        {
            int imported = 0;
            int skipped = 0;

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    // Skip header line
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(';', ',');

                    if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0]))
                    {
                        skipped++;
                        continue;
                    }

                    var participant = new Participant
                    {
                        Name = parts[0].Trim()
                    };

                    // Birth date (column 2)
                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        if (DateTime.TryParse(parts[1].Trim(), out DateTime birthDate))
                        {
                            participant.BirthDate = birthDate;
                        }
                    }

                    // Registration number (column 3)
                    if (parts.Length > 2)
                    {
                        participant.RegistrationNumber = parts[2].Trim();
                    }

                    // Notes (column 4)
                    if (parts.Length > 3)
                    {
                        participant.Notes = parts[3].Trim();
                    }

                    Participants.Add(participant);
                    imported++;
                }
            }

            MessageBox.Show($"Import dokončený!\n\nImportované: {imported}\nPreskočené: {skipped}",
                "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateBulkPdf_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"GenerateBulkPdf_Click STARTED!\n\n" +
                            $"Participants.Count = {Participants.Count}\n" +
                            $"DgParticipants.Items.Count = {DgParticipants.Items.Count}\n" +
                            $"Participants object: {Participants.GetHashCode()}\n" +
                            $"DgParticipants.ItemsSource: {DgParticipants.ItemsSource?.GetHashCode()}",
                            "DEBUG 1");

            MessageBox.Show($"GenerateBulkPdf_Click STARTED!\n\n" +
                            $"Participants.Count = {Participants.Count}\n" +
                            $"DgParticipants.Items.Count = {DgParticipants.Items.Count}\n" +
                            $"Participants object: {Participants.GetHashCode()}\n" +
                            $"DgParticipants.ItemsSource: {DgParticipants.ItemsSource?.GetHashCode()}",
                            "DEBUG 1");

            // Validation
            if (Participants.Count == 0)
            {
                Debug.WriteLine("Prosím pridajte aspoň jedného účastníka.");

                MessageBox.Show("Prosím pridajte aspoň jedného účastníka.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Participants: {Participants.Count}", "DEBUG 2");

            if (string.IsNullOrWhiteSpace(TxtBulkOrganizer.Text))
            {
                MessageBox.Show("Prosím vyplňte organizátora.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Organizer OK", "DEBUG 3");

            var topics = ParseTopics();

            MessageBox.Show($"Topics parsed: {topics.Count}", "DEBUG 4");

            if (topics.Count == 0)
            {
                MessageBox.Show("Prosím zadajte aspoň jednu tému podujatia.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("About to show EventDatesDialog", "DEBUG 5");

            // Show dialog to assign dates to topics
            var eventTopics = topics.Select(t => new EventTopic { Topic = t }).ToList();
            var datesDialog = new EventDatesDialog(eventTopics);

            MessageBox.Show("EventDatesDialog created, about to show", "DEBUG 6");

            if (datesDialog.ShowDialog() != true)
            {
                MessageBox.Show("Dialog was CANCELLED", "DEBUG 7");
                return;
            }

            MessageBox.Show("Dialog OK, getting topics", "DEBUG 8");

            // Get updated topics with dates
            eventTopics = datesDialog.EventTopics;

            MessageBox.Show("About to show FolderBrowserDialog", "DEBUG 9");

            // Folder selection using WPF FolderBrowserDialog alternative
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Vyberte priečinok pre uloženie certifikátov",
                ShowNewFolderButton = true
            };

            MessageBox.Show("FolderBrowserDialog created, about to show", "DEBUG 10");

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show($"Folder selected: {folderDialog.SelectedPath}", "DEBUG 11");

                try
                {
                    GenerateAllPdfs(folderDialog.SelectedPath, eventTopics);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba pri generovaní PDF:\n{ex.Message}",
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Folder selection was CANCELLED", "DEBUG 12");
            }
        }

        private void GenerateAllPdfs(string folderPath, List<EventTopic> topics)
        {
            int successCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            foreach (var participant in Participants)
            {
                foreach (var topic in topics)
                {
                    try
                    {
                        string fileName = $"Certificate_{SanitizeFileName(participant.Name)}_{SanitizeFileName(topic.Topic)}.pdf";
                        string fullPath = IOPath.Combine(folderPath, fileName);

                        System.Diagnostics.Debug.WriteLine($"===== Generating PDF =====");
                        System.Diagnostics.Debug.WriteLine($"File: {fullPath}");
                        System.Diagnostics.Debug.WriteLine($"Participant: {participant.Name}");
                        System.Diagnostics.Debug.WriteLine($"Topic: {topic.Topic}");

                        CreatePdfDocument(fullPath, participant, topic);
                        successCount++;

                        System.Diagnostics.Debug.WriteLine($"SUCCESS!");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;

                        // Získajte celú chybovú hlášku
                        string fullError = ex.ToString(); // Toto dá celý stack trace
                        System.Diagnostics.Debug.WriteLine($"ERROR: {fullError}");

                        string errorMsg = ex.Message;
                        if (ex.InnerException != null)
                        {
                            errorMsg += $"\n\nInner Exception: {ex.InnerException.Message}";
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMsg += $"\n\nInner Inner Exception: {ex.InnerException.InnerException.Message}";
                            }
                        }

                        errors.Add($"{participant.Name} - {topic.Topic}:\n{errorMsg}");
                    }
                }
            }

            // Show results
            var message = $"Generovanie dokončené!\n\n" +
                         $"Úspešne vytvorené: {successCount}\n" +
                         $"Chyby: {errorCount}";

            if (errors.Any())
            {
                message += $"\n\n=== CHYBOVÉ SPRÁVY ===\n";
                foreach (var error in errors.Take(3))
                {
                    message += $"\n{error}\n";
                }
                if (errors.Count > 3)
                {
                    message += $"\n... a ďalších {errors.Count - 3} chýb";
                }
            }

            MessageBox.Show(message, "Výsledok generovania",
                MessageBoxButton.OK,
                errorCount == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (successCount > 0)
            {
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            }
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in IOPath.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace(" ", "_");
        }

        private void CreatePdfDocument(string filePath, Participant participant, EventTopic topic)
        {
            System.Diagnostics.Debug.WriteLine("==> CreatePdfDocument START");

            PageSize pageSize = null;

            try
            {
                pageSize = GetSelectedPageSize();
                System.Diagnostics.Debug.WriteLine($"PageSize: {pageSize.GetWidth()} x {pageSize.GetHeight()}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetSelectedPageSize: {ex.Message}");
                throw new Exception($"Chyba pri určovaní veľkosti papiera: {ex.Message}", ex);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Creating PDF for: {filePath}");

                // Načítame predvolenú šablónu
                var template = _templateRepo.GetDefault();

                // Použijeme helper na generovanie PDF
                CertificateGenerator.Helpers.CertificatePdfGenerator.GeneratePdf(
                    filePath,
                    template,
                    TxtBulkOrganizer.Text,
                    topic.Topic,
                    topic.EventDate,
                    participant.Name,
                    participant.BirthDate,
                    participant.RegistrationNumber,
                    participant.Notes,
                    pageSize
                );

                System.Diagnostics.Debug.WriteLine("==> CreatePdfDocument SUCCESS");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"==> CreatePdfDocument ERROR: {ex.ToString()}");
                throw;
            }
        }


        private PageSize GetSelectedPageSize()
        {
            var selectedItem = CmbBulkPaperFormat.SelectedItem as ComboBoxItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Nie je vybratý formát papiera, použije sa A5.", "Upozornenie");
                return PageSize.A5;
            }

            string format = selectedItem.Content.ToString();

            if (format.StartsWith("A3"))
                return PageSize.A3;
            else if (format.StartsWith("A4"))
                return PageSize.A4;
            else
                return PageSize.A5;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Participant model class
    public class Participant
    {
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string Notes { get; set; }
    }
}
