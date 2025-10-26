using System;
using System.Diagnostics;
using System.Windows;

namespace CertificateGenerator.Data
{
    /// <summary>
    /// Helper metódy pre prácu s databázami v UI
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Otvorí priečinok s databázami v Prieskumníkovi
        /// </summary>
        public static void OpenDatabaseFolder(DatabaseManager dbManager)
        {
            try
            {
                string folderPath = dbManager.GetDatabaseFolder();
                Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Nepodarilo sa otvoriť priečinok:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exportuje aktuálnu databázu
        /// </summary>
        public static void ExportDatabase(DatabaseManager dbManager)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Exportovať databázu",
                    Filter = "SQLite databáza (*.db)|*.db",
                    FileName = $"CertificateGenerator_export_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    dbManager.BackupDatabase(saveDialog.FileName);
                    MessageBox.Show(
                        "Databáza bola úspešne exportovaná.",
                        "Export dokončený",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri exportovaní databázy:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Importuje databázu
        /// </summary>
        public static void ImportDatabase(DatabaseManager dbManager)
        {
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Importovať databázu",
                    Filter = "SQLite databáza (*.db)|*.db",
                    Multiselect = true
                };

                if (openDialog.ShowDialog() == true)
                {
                    string targetFolder = dbManager.GetDatabaseFolder();
                    int imported = 0;

                    foreach (string file in openDialog.FileNames)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        string targetPath = System.IO.Path.Combine(targetFolder, fileName);

                        System.IO.File.Copy(file, targetPath, true);
                        imported++;
                    }

                    MessageBox.Show(
                        $"Úspešne importovaných {imported} databáz.\n\n" +
                        "Reštartujte aplikáciu pre načítanie importovaných databáz.",
                        "Import dokončený",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba pri importovaní databázy:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Zobrazí dialog pre zlúčenie databáz
        /// </summary>
        public static bool ShowMergeDatabasesDialog(DatabaseManager dbManager)
        {
            var foreignDbs = dbManager.GetForeignDatabases();

            if (foreignDbs.Count == 0)
            {
                MessageBox.Show(
                    "Neboli nájdené žiadne databázy z iných počítačov.",
                    "Zlúčenie databáz",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return false;
            }

            string message = $"Bolo detekovaných {foreignDbs.Count} databáz z iných počítačov.\n\n" +
                           "Chcete ich zlúčiť do jednej novej databázy?\n\n" +
                           "Poznámka:\n" +
                           "• Všetky záznamy budú skopírované do novej databázy\n" +
                           "• Duplikáty budú zachované\n" +
                           "• Pred zlúčením bude vytvorená záloha\n" +
                           "• Staré databázy budú vymazané";

            var result = MessageBox.Show(
                message,
                "Zlúčenie databáz",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var mergeResult = dbManager.MergeDatabases();

                    if (mergeResult.Success)
                    {
                        MessageBox.Show(
                            $"{mergeResult.Message}\n\n" +
                            $"Záloha vytvorená: {System.IO.Path.GetFileName(mergeResult.BackupPath)}\n\n" +
                            "Aplikácia sa teraz reštartuje pre načítanie zlúčenej databázy.",
                            "Zlúčenie úspešné",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(
                            mergeResult.Message,
                            "Zlúčenie zlyhalo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Chyba pri zlúčení databáz:\n{ex.Message}",
                        "Chyba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Skontroluje cudzie databázy pri štarte
        /// </summary>
        public static void CheckForForeignDatabasesOnStartup(DatabaseManager dbManager)
        {
            if (dbManager.HasForeignDatabases())
            {
                var result = MessageBox.Show(
                    "Boli detekované databázy z iných počítačov.\n\n" +
                    "Chcete ich teraz zlúčiť do jednej databázy?",
                    "Detekované cudzie databázy",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ShowMergeDatabasesDialog(dbManager);
                }
            }
        }
    }
}
