using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Input;

namespace Pharmacy_Management_System.Views
{
    public partial class LoginWindow : Window
    {
        private bool passwordVisible = false;
        private bool isWindowLoaded = false;

        public LoginWindow()
        {
            try
            {
                InitializeComponent();
                this.Loaded += LoginWindow_Loaded;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoginWindow error: {ex.Message}");
                throw;
            }
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                isWindowLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loaded error: {ex.Message}");
            }
        }

        private void Panel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try { this.DragMove(); } catch { }
            }
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.WindowState == WindowState.Normal)
                {
                    this.MaxHeight = SystemParameters.WorkArea.Height;
                    this.MaxWidth = SystemParameters.WorkArea.Width;
                    this.ResizeMode = ResizeMode.CanResize;
                    this.WindowState = WindowState.Maximized;
                    BtnMax.Content = "❐";

                    if (RootCard != null)
                    {
                        RootCard.Width = double.NaN;
                        RootCard.Height = double.NaN;
                        RootCard.HorizontalAlignment = HorizontalAlignment.Stretch;
                        RootCard.VerticalAlignment = VerticalAlignment.Stretch;
                        RootCard.CornerRadius = new System.Windows.CornerRadius(0);
                    }
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                    BtnMax.Content = "◻";

                    if (RootCard != null)
                    {
                        RootCard.Width = 1020;
                        RootCard.Height = 610;
                        RootCard.HorizontalAlignment = HorizontalAlignment.Center;
                        RootCard.VerticalAlignment = VerticalAlignment.Center;
                        RootCard.CornerRadius = new System.Windows.CornerRadius(0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnMax error: {ex.Message}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LangCombo_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isWindowLoaded) return;
            try
            {
                var combo = sender as System.Windows.Controls.ComboBox;
                if (combo?.SelectedItem is System.Windows.Controls.ComboBoxItem item)
                {
                    var tag = item.Tag?.ToString();
                    if (string.IsNullOrEmpty(tag)) return;

                    // RTL support
                    if (tag == "ps" || tag == "da" || tag == "ur" || tag == "ar")
                        this.FlowDirection = FlowDirection.RightToLeft;
                    else
                        this.FlowDirection = FlowDirection.LeftToRight;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LangCombo error: {ex.Message}");
            }
        }

        private void BtnShow_Click(object sender, RoutedEventArgs e)
        {
            passwordVisible = !passwordVisible;
            if (passwordVisible)
            {
                txtPasswordReveal.Text = pwdBox.Password;
                txtPasswordReveal.Visibility = Visibility.Visible;
                pwdBox.Visibility = Visibility.Collapsed;
                BtnShow.Content = "🙈";
            }
            else
            {
                pwdBox.Password = txtPasswordReveal.Text;
                txtPasswordReveal.Visibility = Visibility.Collapsed;
                pwdBox.Visibility = Visibility.Visible;
                BtnShow.Content = "👁";
            }
        }

        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text?.Trim();
            var password = passwordVisible ? txtPasswordReveal.Text : pwdBox.Password;

            HideError();

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Username is required!");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Password is required!");
                return;
            }

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                using var cmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Users WHERE Username=@user AND Password=@pass",
                    conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", password);
                long count = (long)cmd.ExecuteScalar()!;

                if (count > 0)
                {
                    var main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Invalid username or password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Connection error: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
            if (borderError != null)
                borderError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            txtError.Visibility = Visibility.Collapsed;
            if (borderError != null)
                borderError.Visibility = Visibility.Collapsed;
        }
    }
}