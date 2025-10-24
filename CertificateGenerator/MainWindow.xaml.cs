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

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in IOPath.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace(" ", "_");
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

        private void QuickGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var quickWindow = new QuickGenerationWindow();
                quickWindow.Owner = this;
                quickWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní okna:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


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
