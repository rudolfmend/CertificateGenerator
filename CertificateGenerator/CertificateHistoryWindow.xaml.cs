using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class CertificateHistoryWindow : Window
    {
        private CertificateRepository _certificateRepo;
        private List<CertificateModel> _allCertificates;
        private List<CertificateModel> _filteredCertificates;

        public CertificateHistoryWindow()
        {
            InitializeComponent();
            InitializeRepository();
            LoadCertificates();
        }

        private void InitializeRepository()
        {
            try
            {
                var dbManager = App.DatabaseManager;
                _certificateRepo = new CertificateRepository(dbManager);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCertificates()
        {
            try
            {
                _allCertificates = _certificateRepo.GetAll();
                _filteredCertificates = new List<CertificateModel>(_allCertificates);

                DgCertificates.ItemsSource = _filteredCertificates;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri načítaní certifikátov:\n{ex.Message}",
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

        private void UpdateStatistics()
        {
            TxtTotalCount.Text = _allCertificates.Count.ToString();
            TxtFilteredCount.Text = _filteredCertificates.Count.ToString();
        }

        private void ApplyFilters()
        {
            string searchTerm = TxtSearch.Text?.Trim().ToLower() ?? "";
            DateTime? dateFrom = DtpDateFrom.SelectedDate;
            DateTime? dateTo = DtpDateTo.SelectedDate;

            _filteredCertificates = _allCertificates.Where(c =>
            {
                // Text search
                bool matchesSearch = string.IsNullOrEmpty(searchTerm) ||
                    c.ParticipantName.ToLower().Contains(searchTerm) ||
                    c.OrganizerName.ToLower().Contains(searchTerm) ||
                    c.EventTopicName.ToLower().Contains(searchTerm);

                // Date from
                bool matchesDateFrom = !dateFrom.HasValue ||
                    c.CreatedAt >= dateFrom.Value;

                // Date to
                bool matchesDateTo = !dateTo.HasValue ||
                    c.CreatedAt <= dateTo.Value.AddDays(1).AddSeconds(-1);

                return matchesSearch && matchesDateFrom && matchesDateTo;
            }).ToList();

            DgCertificates.ItemsSource = _filteredCertificates;
            UpdateStatistics();
        }

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            DtpDateFrom.SelectedDate = null;
            DtpDateTo.SelectedDate = null;
            ApplyFilters();
        }

        private void DgCertificates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFile_Click(sender, e);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (DgCertificates.SelectedItem is CertificateModel certificate)
            {
                if (!string.IsNullOrEmpty(certificate.FilePath))
                {
                    if (File.Exists(certificate.FilePath))
                    {
                        try
                        {
                            Process.Start(certificate.FilePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Chyba pri otváraní súboru:\n{ex.Message}",
                                "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Súbor nebol nájdený. Možno bol presunutý alebo odstránený.",
                            "Súbor nenájdený", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Pre tento certifikát nie je uložená cesta k súboru.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte certifikát zo zoznamu.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (DgCertificates.SelectedItem is CertificateModel certificate)
            {
                if (!string.IsNullOrEmpty(certificate.FilePath))
                {
                    string folderPath = Path.GetDirectoryName(certificate.FilePath);

                    if (Directory.Exists(folderPath))
                    {
                        try
                        {
                            Process.Start("explorer.exe", folderPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Chyba pri otváraní priečinka:\n{ex.Message}",
                                "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Priečinok nebol nájdený.",
                            "Priečinok nenájdený", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Pre tento certifikát nie je uložená cesta k súboru.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte certifikát zo zoznamu.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (DgCertificates.SelectedItem is CertificateModel certificate)
            {
                var result = MessageBox.Show(
                    $"Naozaj chcete odstrániť záznam o certifikáte pre '{certificate.ParticipantName}'?\n\n" +
                    "Poznámka: Toto odstráni len záznam z databázy, nie samotný súbor PDF.",
                    "Potvrdenie odstránenia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _certificateRepo.Delete(certificate.Id);

                        MessageBox.Show("Záznam bol úspešne odstránený.",
                            "Úspech", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadCertificates();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Chyba pri odstraňovaní záznamu:\n{ex.Message}",
                            "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Prosím vyberte certifikát zo zoznamu.",
                    "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PrintFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Vyberte priečinok s PDF certifikátmi na tlač"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var pdfFiles = Directory.GetFiles(dialog.SelectedPath, "*.pdf");

                if (pdfFiles.Length == 0)
                {
                    MessageBox.Show("V priečinku nie sú žiadne PDF súbory.",
                        "Informácia", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var orientationResult = MessageBox.Show(
                    $"Nájdených {pdfFiles.Length} PDF súborov.\n\n" +
                    $"Zvoľte orientáciu:\n\n" +
                    $"ÁNO = Šírka (Landscape)\n" +
                    $"NIE = Výška (Portrait)",
                    "Nastavenie tlače",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (orientationResult == MessageBoxResult.Cancel)
                    return;

                bool isLandscape = (orientationResult == MessageBoxResult.Yes);

                int sent = 0;
                foreach (var pdf in pdfFiles)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = pdf,
                            Verb = "print",
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        Process.Start(psi);
                        sent++;
                        System.Threading.Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Chyba: {ex.Message}");
                    }
                }

                MessageBox.Show(
                    $"Odoslaných na tlač: {sent} z {pdfFiles.Length} certifikátov.\n\n" +
                    $"Orientácia: {(isLandscape ? "Šírka" : "Výška")}\n\n" +
                    $"DÔLEŽITÉ: Ak orientácia nie je správna:\n" +
                    $"1. Ovládací panel → Zariadenia a tlačiarne\n" +
                    $"2. HP LaserJet M12a → Vlastnosti → Predvoľby\n" +
                    $"3. Nastavte orientáciu a uložte ako predvolenú",
                    "Tlač dokončená",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
