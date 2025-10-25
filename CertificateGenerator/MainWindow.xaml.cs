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
                MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBulkGeneration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bulkWindow = new BulkGenerationWindow("", 2);
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

        private void OpenTemplateEditor_Click(object sender, RoutedEventArgs e)
        {
            TemplateEditorWindow editorWindow = new TemplateEditorWindow();
            this.Hide();

            editorWindow.Closed += (s, args) =>
            {
                this.Show();
            };

            editorWindow.Show();
        }

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

        // ========== MANAGEMENT WINDOWS - REFACTORED ==========

        private void ManageParticipants_Click(object sender, RoutedEventArgs e)
        {
            var participantWindow = new ParticipantManagerWindow(App.DatabaseManager);
            participantWindow.Owner = this;
            participantWindow.ShowDialog();
        }

        /// <summary>
        /// UPDATED: Používa nové ManageOrganizerWindow namiesto SelectOrganizerWindow
        /// </summary>
        private void ManageOrganizers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageWindow = new ManageOrganizerWindow(App.DatabaseManager, selectionMode: false);
                manageWindow.Owner = this;
                manageWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní správy organizátorov:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// UPDATED: Používa nové ManageEventTopicWindow namiesto SelectEventTopicWindow
        /// </summary>
        private void ManageEventTopics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageWindow = new ManageEventTopicWindow(App.DatabaseManager, selectionMode: false);
                manageWindow.Owner = this;
                manageWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní správy tém:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CertificateHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var historyWindow = new CertificateHistoryWindow();
                historyWindow.Owner = this;
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri otváraní histórie:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
