using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Pharmacy_Management_System.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                // Create settings table if not exists
                using var createCmd = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT
                    )", conn);
                createCmd.ExecuteNonQuery();

                // Load settings
                using var cmd = new SqliteCommand(
                    "SELECT Key, Value FROM Settings", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string key = reader["Key"]?.ToString() ?? "";
                    string val = reader["Value"]?.ToString() ?? "";
                    switch (key)
                    {
                        case "PharmacyName": txtPharmacyName.Text = val; break;
                        case "OwnerName": txtOwnerName.Text = val; break;
                        case "Phone": txtPhone.Text = val; break;
                        case "Address": txtAddress.Text = val; break;
                        case "License": txtLicense.Text = val; break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void SaveSetting(string key, string value)
        {
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(@"
                INSERT OR REPLACE INTO Settings (Key, Value) 
                VALUES (@key, @value)", conn);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
        }

        private void btnSavePharmacy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSetting("PharmacyName", txtPharmacyName.Text);
                SaveSetting("OwnerName", txtOwnerName.Text);
                SaveSetting("Phone", txtPhone.Text);
                SaveSetting("Address", txtAddress.Text);
                SaveSetting("License", txtLicense.Text);

                MessageBox.Show("Pharmacy information saved successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            txtPassError.Visibility = Visibility.Collapsed;
            txtPassSuccess.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(txtCurrentPass.Password))
            {
                txtPassError.Text = "Please enter current password!";
                txtPassError.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewPass.Password))
            {
                txtPassError.Text = "Please enter new password!";
                txtPassError.Visibility = Visibility.Visible;
                return;
            }

            if (txtNewPass.Password != txtConfirmPass.Password)
            {
                txtPassError.Text = "New passwords do not match!";
                txtPassError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                // Verify current password
                using var checkCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Users WHERE Username='admin' AND Password=@pass",
                    conn);
                checkCmd.Parameters.AddWithValue("@pass", txtCurrentPass.Password);
                long count = (long)checkCmd.ExecuteScalar()!;

                if (count == 0)
                {
                    txtPassError.Text = "Current password is incorrect!";
                    txtPassError.Visibility = Visibility.Visible;
                    return;
                }

                // Update password
                using var updateCmd = new SqliteCommand(
                    "UPDATE Users SET Password=@pass WHERE Username='admin'", conn);
                updateCmd.Parameters.AddWithValue("@pass", txtNewPass.Password);
                updateCmd.ExecuteNonQuery();

                txtCurrentPass.Clear();
                txtNewPass.Clear();
                txtConfirmPass.Clear();
                txtPassSuccess.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                txtPassError.Text = $"Error: {ex.Message}";
                txtPassError.Visibility = Visibility.Visible;
            }
        }

        private void btnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Save Backup",
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
                    FileName = $"PharmacyBackup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    string dbPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "PharmacyMS", "pharmacy.db");

                    File.Copy(dbPath, saveDialog.FileName, true);

                    txtBackupStatus.Text = $"✅ Backup created successfully at: {saveDialog.FileName}";
                    txtBackupStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(46, 125, 50));
                    txtBackupStatus.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                txtBackupStatus.Text = $"❌ Backup failed: {ex.Message}";
                txtBackupStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(198, 40, 40));
                txtBackupStatus.Visibility = Visibility.Visible;
            }
        }
        private void btnLanguage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as System.Windows.Controls.Button;
                if (btn?.Tag == null) return;

                string lang = btn.Tag.ToString() ?? "en";
                Helpers.AppLanguage.SetLanguage(lang);

                // Reset all buttons safely
                var blue = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(21, 101, 192));
                var gray = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(245, 245, 245));
                var white = System.Windows.Media.Brushes.White;
                var dark = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 51, 51));

                if (btnLangEn != null) { btnLangEn.Background = gray; btnLangEn.Foreground = dark; }
                if (btnLangPs != null) { btnLangPs.Background = gray; btnLangPs.Foreground = dark; }
                if (btnLangDa != null) { btnLangDa.Background = gray; btnLangDa.Foreground = dark; }
                if (btnLangUr != null) { btnLangUr.Background = gray; btnLangUr.Foreground = dark; }
                if (btnLangAr != null) { btnLangAr.Background = gray; btnLangAr.Foreground = dark; }

                // Highlight selected
                btn.Background = blue;
                btn.Foreground = white;

                string langName = lang switch
                {
                    "ps" => "پښتو",
                    "da" => "دری",
                    "ur" => "اردو",
                    "ar" => "العربية",
                    _ => "English"
                };

                if (txtLangStatus != null)
                    txtLangStatus.Text = $"Current Language: {langName}";

                // Save language
                SaveSetting("Language", lang);

                MessageBox.Show($"Language changed to {langName}!",
                    "Language Changed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            // MainWindow update karo
            try
            {
                var mainWindow = Application.Current.Windows
                    .OfType<MainWindow>()
                    .FirstOrDefault();

                if (mainWindow != null)
                {
                    mainWindow.UpdateLanguage();
                }
            }
            catch { }
        }
        private void btnRestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Select Backup File",
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show(
                        "Are you sure? This will replace your current data!",
                        "Confirm Restore",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        string dbPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "PharmacyMS", "pharmacy.db");

                        File.Copy(openDialog.FileName, dbPath, true);

                        txtBackupStatus.Text = "✅ Backup restored successfully! Please restart the application.";
                        txtBackupStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(46, 125, 50));
                        txtBackupStatus.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                txtBackupStatus.Text = $"❌ Restore failed: {ex.Message}";
                txtBackupStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(198, 40, 40));
                txtBackupStatus.Visibility = Visibility.Visible;
            }
        }

        private void txtPharmacyName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}