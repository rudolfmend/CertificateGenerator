using System;
using System.Threading;
using System.Windows;
using CertificateGenerator.Data;
using CertificateGenerator.Helpers;
using CertificateGenerator.Converters;

namespace CertificateGenerator
{
    public partial class App : Application
    {
        public static DatabaseManager DatabaseManager { get; private set; }
        private static Mutex _mutex = null;
        private const string MutexName = "Global\\CertificateGeneratorSingleInstance";

        protected override void OnStartup(StartupEventArgs e)
        {
            // Kontrola či aplikácia už beží
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show(
                    "The CertificateGenerator application is already running.\n\n" +
                    "You can only have one instance of the application open.",
                    "The application is already running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // Inicializácia databázy
            try
            {
                DatabaseManager = new DatabaseManager();

                // Aktualizuj preset šablóny pri každom štarte
                var templateRepo = new CertificateTemplateRepository(DatabaseManager);
                templateRepo.ReloadPresetsToDatabase();

                System.Diagnostics.Debug.WriteLine("[App.Startup] Preset šablóny boli aktualizované");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Kritická chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
            base.OnExit(e);
        }
    }
}
