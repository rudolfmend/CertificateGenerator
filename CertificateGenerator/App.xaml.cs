using System;
using System.Windows;
using CertificateGenerator.Data;

namespace CertificateGenerator
{
    public partial class App : Application
    {
        public static DatabaseManager DatabaseManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inicializácia databázy
            try
            {
                DatabaseManager = new DatabaseManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pri inicializácii databázy:\n{ex.Message}",
                    "Kritická chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
