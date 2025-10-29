using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
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
        private ParticipantRepository _participantRepo;
        private OrganizerRepository _organizerRepo;
        private EventTopicRepository _topicRepo;
        private CertificateRepository _certificateRepo;
        private bool _hasUnsavedChanges = false;

        public BulkGenerationWindow()
        {
            InitializeComponent();
            Participants = new ObservableCollection<Participant>();
            DgParticipants.ItemsSource = Participants;
            Participants.CollectionChanged += (s, e) => {
                UpdateStatistics();
                _hasUnsavedChanges = Participants.Count > 0 || !string.IsNullOrWhiteSpace(TxtBulkTopics.Text);
            };
            InitializeRepositories();
        }

        public BulkGenerationWindow(string organizer, int paperFormatIndex)
            : this()
        {
            TxtBulkOrganizer.Text = organizer;
            CmbBulkPaperFormat.SelectedIndex = paperFormatIndex;
            TxtBulkOrganizer.IsReadOnly = false;
            TxtBulkOrganizer.IsEnabled = true;
        }

        private void InitializeRepositories()
        {
            var dbManager = App.DatabaseManager;
            _templateRepo = new CertificateTemplateRepository(dbManager);
            _participantRepo = new ParticipantRepository(dbManager);
            _organizerRepo = new OrganizerRepository(dbManager);
            _topicRepo = new EventTopicRepository(dbManager);
            _certificateRepo = new CertificateRepository(dbManager);
        }

        private void UpdateStatistics()
        {
            TxtParticipantCount.Text = Participants.Count.ToString();
            UpdatePdfCount();
        }

        private void ManageParticipants_Click(object sender, RoutedEventArgs e)
        {
            var participantWindow = new ParticipantManagerWindow(App.DatabaseManager);
            if (participantWindow.ShowDialog() == true && participantWindow.SelectedParticipant != null)
            {
                var selected = participantWindow.SelectedParticipant;

                // Check if already in list
                if (!Participants.Any(p => p.Name == selected.Name))
                {
                    Participants.Add(new Participant
                    {
                        Name = selected.Name,
                        BirthDate = selected.BirthDate,
                        RegistrationNumber = selected.RegistrationNumber,
                        Notes = selected.Notes
                    });
                }
            }
        }

        private void ManageOrganizers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageWindow = new ManageOrganizerWindow(App.DatabaseManager, selectionMode: true);
                manageWindow.Owner = this;
                if (manageWindow.ShowDialog() == true && manageWindow.SelectedOrganizer != null)
                {
                    TxtBulkOrganizer.Text = manageWindow.SelectedOrganizer.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní správy organizátorov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtBulkTopics_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePdfCount();
            _hasUnsavedChanges = Participants.Count > 0 || !string.IsNullOrWhiteSpace(TxtBulkTopics.Text);
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
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        topics.Add(trimmed);
                }
            }

            return topics;
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

                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        if (DateTime.TryParse(parts[1].Trim(), out DateTime birthDate))
                            participant.BirthDate = birthDate;
                    }

                    if (parts.Length > 2)
                        participant.RegistrationNumber = parts[2].Trim();

                    if (parts.Length > 3)
                        participant.Notes = parts[3].Trim();

                    Participants.Add(participant);

                    // Ulož do databázy
                    try
                    {
                        var participantModel = new ParticipantModel
                        {
                            Name = participant.Name,
                            BirthDate = participant.BirthDate,
                            RegistrationNumber = participant.RegistrationNumber,
                            Notes = participant.Notes
                        };
                        _participantRepo.Add(participantModel);
                        imported++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Chyba pri ukladaní účastníka: {ex.Message}");
                        skipped++;
                    }
                }
            }

            MessageBox.Show($"Import dokončený!\n\nImportované: {imported}\nPreskočené: {skipped}",
                "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SelectTopicsFromDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var topicRepo = new EventTopicRepository(App.DatabaseManager);
                var allTopics = topicRepo.GetAll();

                if (allTopics.Count == 0)
                {
                    MessageBox.Show("Nemáte žiadne uložené témy v databáze.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var selectDialog = new SelectMultipleTopicsWindow(allTopics);
                if (selectDialog.ShowDialog() == true)
                {
                    var selectedTopics = string.Join("\n", selectDialog.SelectedTopics.Select(t => t.Topic));

                    // Pridaj k existujúcemu textu alebo nahraď
                    if (string.IsNullOrWhiteSpace(TxtBulkTopics.Text))
                        TxtBulkTopics.Text = selectedTopics;
                    else
                        TxtBulkTopics.Text += "\n" + selectedTopics;

                    MessageBox.Show($"Pridaných {selectDialog.SelectedTopics.Count} tém.",
                        "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní tém:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateBulkPdf_Click(object sender, RoutedEventArgs e)
        {
            if (Participants.Count == 0)
            {
                MessageBox.Show("Prosím pridajte aspoň jedného účastníka.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtBulkOrganizer.Text))
            {
                MessageBox.Show("Prosím vyplňte organizátora.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var topics = ParseTopics();
            if (topics.Count == 0)
            {
                MessageBox.Show("Prosím zadajte aspoň jednu tému podujatia.",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var eventTopics = topics.Select(t => new EventTopic { Topic = t }).ToList();
            var datesDialog = new EventDatesDialog(eventTopics);

            if (datesDialog.ShowDialog() != true)
                return;

            eventTopics = datesDialog.EventTopics;

            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Vyberte priečinok pre uloženie certifikátov",
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
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
        }

        private void GenerateAllPdfs(string folderPath, List<EventTopic> topics)
        {
            int successCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            // Ulož organizátora do DB
            int organizerId = 0;
            string organizerName = TxtBulkOrganizer.Text.Trim();

            try
            {
                var existingOrganizers = _organizerRepo.Search(organizerName);
                var exactMatch = existingOrganizers.FirstOrDefault(o =>
                    o.Name.Equals(organizerName, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    organizerId = exactMatch.Id;
                    _organizerRepo.IncrementUsage(organizerId);
                }
                else
                {
                    var organizer = new OrganizerModel { Name = organizerName };
                    organizerId = _organizerRepo.Add(organizer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri ukladaní organizátora: {ex.Message}");
            }

            // Ulož témy do DB a získaj ich ID
            var topicIds = new Dictionary<string, int>();
            foreach (var topic in topics)
            {
                try
                {
                    var existingTopics = _topicRepo.Search(topic.Topic);
                    var exactMatch = existingTopics.FirstOrDefault(t =>
                        t.Topic.Equals(topic.Topic, StringComparison.OrdinalIgnoreCase));

                    if (exactMatch != null)
                    {
                        topicIds[topic.Topic] = exactMatch.Id;
                        _topicRepo.IncrementUsage(exactMatch.Id);
                    }
                    else
                    {
                        var topicModel = new EventTopicModel { Topic = topic.Topic };
                        int topicId = _topicRepo.Add(topicModel);
                        topicIds[topic.Topic] = topicId;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Chyba pri ukladaní témy: {ex.Message}");
                    topicIds[topic.Topic] = 0;
                }
            }

            // Generuj PDF pre každého účastníka a každú tému
            foreach (var participant in Participants)
            {
                // Získaj alebo vytvor účastníka v DB
                int participantId = 0;
                try
                {
                    var existingParticipants = _participantRepo.Search(participant.Name);
                    var exactMatch = existingParticipants.FirstOrDefault(p =>
                        p.Name.Equals(participant.Name, StringComparison.OrdinalIgnoreCase) &&
                        p.BirthDate == participant.BirthDate);

                    if (exactMatch != null)
                    {
                        participantId = exactMatch.Id;
                        _participantRepo.IncrementUsage(participantId);
                    }
                    else
                    {
                        var participantModel = new ParticipantModel
                        {
                            Name = participant.Name,
                            BirthDate = participant.BirthDate,
                            RegistrationNumber = participant.RegistrationNumber,
                            Notes = participant.Notes
                        };
                        participantId = _participantRepo.Add(participantModel);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Chyba pri ukladaní účastníka: {ex.Message}");
                }

                foreach (var topic in topics)
                {
                    try
                    {
                        string fileName = $"Certificate_{SanitizeFileName(participant.Name)}_{SanitizeFileName(topic.Topic)}.pdf";
                        string fullPath = IOPath.Combine(folderPath, fileName);

                        CreatePdfDocument(fullPath, participant, topic);
                        successCount++;

                        // Ulož certifikát do histórie
                        try
                        {
                            var certificate = new CertificateModel
                            {
                                OrganizerId = organizerId,
                                OrganizerName = organizerName,
                                ParticipantId = participantId,
                                ParticipantName = participant.Name,
                                ParticipantBirthDate = participant.BirthDate,
                                ParticipantRegistrationNumber = participant.RegistrationNumber,
                                EventTopicId = topicIds.ContainsKey(topic.Topic) ? topicIds[topic.Topic] : 0,
                                EventTopicName = topic.Topic,
                                EventDate = topic.EventDate,
                                Notes = participant.Notes,
                                PaperFormat = GetSelectedPaperFormat(),
                                FilePath = fullPath
                            };
                            _certificateRepo.Add(certificate);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Chyba pri ukladaní certifikátu do histórie: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        string errorMsg = ex.Message;
                        if (ex.InnerException != null)
                            errorMsg += $"\n\nInner: {ex.InnerException.Message}";
                        errors.Add($"{participant.Name} - {topic.Topic}:\n{errorMsg}");
                    }
                }
            }

            var message = $"Generovanie dokončené!\n\n" +
                         $"Úspešne vytvorené: {successCount}\n" +
                         $"Chyby: {errorCount}";

            if (errors.Any())
            {
                message += $"\n\n=== CHYBOVÉ SPRÁVY ===\n";
                foreach (var error in errors.Take(3))
                    message += $"\n{error}\n";
                if (errors.Count > 3)
                    message += $"\n... a ďalších {errors.Count - 3} chýb";
            }

            MessageBox.Show(message, "Výsledok generovania",
                MessageBoxButton.OK,
                errorCount == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (successCount > 0)
            {
                _hasUnsavedChanges = false; // Reset po úspešnom generovaní
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            }
            System.Diagnostics.Debug.WriteLine("Bulk PDF generation completed.");
        }

        private string GetSelectedPaperFormat()
        {
            var selectedItem = CmbBulkPaperFormat.SelectedItem as ComboBoxItem;
            string format = selectedItem?.Content.ToString() ?? "A5";

            if (format.StartsWith("A3")) return "A3";
            if (format.StartsWith("A4")) return "A4";
            return "A5";
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in IOPath.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return fileName.Replace(" ", "_");
        }

        private void CreatePdfDocument(string filePath, Participant participant, EventTopic topic)
        {
            PageSize pageSize = GetSelectedPageSize();
            var template = ModernTemplatePresets.GetOrnamentalLuxuryPreset().Template;

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
        }

        private PageSize GetSelectedPageSize()
        {
            var selectedItem = CmbBulkPaperFormat.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
                return PageSize.A5;

            string format = selectedItem.Content.ToString();


            PageSize size;
            if (format.StartsWith("A3"))
                size = PageSize.A3;
            else if (format.StartsWith("A4"))
                size = PageSize.A4;
            else
                size = PageSize.A5;

            // Otočenie na šírku ak je zaškrtnuté
            if (ChkBulkLandscape?.IsChecked == true)
            {
                size = size.Rotate();
            }

            return size;
        }

        private void SelectFromDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var participantRepo = new ParticipantRepository(App.DatabaseManager);
                var allParticipants = participantRepo.GetAll();

                if (allParticipants.Count == 0)
                {
                    Debug.WriteLine("Nemáte žiadnych uložených účastníkov v databáze.");
                    MessageBox.Show("Nemáte žiadnych uložených účastníkov v databáze.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Vytvor dialógové okno pre výber
                var selectDialog = new SelectMultipleParticipantsWindow(allParticipants);
                if (selectDialog.ShowDialog() == true)
                {
                    // Pridaj vybraných účastníkov do zoznamu
                    foreach (var participant in selectDialog.SelectedParticipants)
                    {
                        // Skontroluj duplikáty
                        if (!Participants.Any(p => p.Name == participant.Name &&
                                                  p.BirthDate == participant.BirthDate))
                        {
                            Participants.Add(new Participant
                            {
                                Name = participant.Name,
                                BirthDate = participant.BirthDate,
                                RegistrationNumber = participant.RegistrationNumber,
                                Notes = participant.Notes
                            });
                        }
                    }
                    Debug.WriteLine($"Pridaných {selectDialog.SelectedParticipants.Count} účastníkov.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba pri načítaní účastníkov:\n{ex.Message} \n + " +
                    ex.ToString());
                MessageBox.Show($"Chyba pri načítaní účastníkov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class Participant
    {
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string Notes { get; set; }
    }
}
