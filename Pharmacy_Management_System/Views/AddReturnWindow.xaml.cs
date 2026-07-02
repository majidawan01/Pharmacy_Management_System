using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pharmacy_Management_System.Views
{
    public partial class AddReturnWindow : Window
    {
        private List<ReturnMedicine> _medicines = new();
        private ReturnMedicine? _selectedMedicine = null;

        public AddReturnWindow()
        {
            InitializeComponent();
            dpReturnDate.SelectedDate = DateTime.Today;
            txtVoucherNo.Text = $"RET-{DateTime.Now:yyyyMMdd-HHmmss}";
            this.Loaded += (s, e) =>
            {
                LoadMedicines();
                txtMedicineSearch.Focus();
            };
        }

        private void LoadMedicines()
        {
            try
            {
                _medicines = new List<ReturnMedicine>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT Id, Name FROM Medicines ORDER BY Name", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _medicines.Add(new ReturnMedicine
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtMedicineSearch_TextChanged(object sender,
            TextChangedEventArgs e)
        {
            try
            {
                string search = txtMedicineSearch.Text.Trim().ToLower();

                if (string.IsNullOrEmpty(search))
                {
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                    _selectedMedicine = null;
                    return;
                }

                var filtered = _medicines
                    .Where(m => m.Name.ToLower().Contains(search))
                    .Take(10)
                    .ToList();

                if (filtered.Count > 0)
                {
                    lstMedicineSuggestions.ItemsSource = filtered
                        .Select(m => m.Name).ToList();
                    lstMedicineSuggestions.Visibility = Visibility.Visible;
                }
                else
                {
                    lstMedicineSuggestions.ItemsSource = null;
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void txtMedicineSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    if (lstMedicineSuggestions.Items.Count > 0)
                    {
                        lstMedicineSuggestions.Focus();
                        lstMedicineSuggestions.SelectedIndex = 0;
                    }
                }
                else if (e.Key == Key.Enter)
                {
                    if (lstMedicineSuggestions.Items.Count == 1)
                    {
                        lstMedicineSuggestions.SelectedIndex = 0;
                        SelectMedicine();
                    }
                    else if (lstMedicineSuggestions.Items.Count > 1)
                    {
                        lstMedicineSuggestions.Focus();
                        lstMedicineSuggestions.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private void lstMedicineSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    SelectMedicine();
                else if (e.Key == Key.Escape)
                {
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                    txtMedicineSearch.Focus();
                }
            }
            catch { }
        }

        private void lstMedicineSuggestions_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            try
            {
                if (lstMedicineSuggestions.SelectedItem == null) return;

                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    SelectMedicine();
            }
            catch { }
        }

        private void SelectMedicine()
        {
            try
            {
                if (lstMedicineSuggestions.SelectedItem == null) return;

                string name = lstMedicineSuggestions.SelectedItem.ToString() ?? "";
                _selectedMedicine = _medicines.FirstOrDefault(m => m.Name == name);

                if (_selectedMedicine != null)
                {
                    txtMedicineSearch.Text = _selectedMedicine.Name;
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                    txtAmount.Focus();
                    txtAmount.SelectAll();
                }
            }
            catch { }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartyName.Text))
            {
                txtError.Text = "Party name is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (!double.TryParse(txtAmount.Text, out double amount))
                amount = 0;

            string type = (cmbType.SelectedItem as ComboBoxItem)?.Content?.ToString()
                ?? "Customer Return";

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"INSERT INTO Returns
                    (Type, VoucherNo, PartyName, TotalAmount, ReturnDate, Notes)
                    VALUES (@type, @vno, @party, @amount, @date, @notes)";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@vno", txtVoucherNo.Text);
                cmd.Parameters.AddWithValue("@party", txtPartyName.Text.Trim());
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@date",
                    dpReturnDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                    DateTime.Today.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@notes", txtNotes.Text.Trim());
                cmd.ExecuteNonQuery();

                if (_selectedMedicine != null)
                {
                    string stockSql = type == "Customer Return"
                        ? "UPDATE Medicines SET Stock = Stock + 1 WHERE Id = @id"
                        : "UPDATE Medicines SET Stock = Stock - 1 WHERE Id = @id";
                    using var stockCmd = new SqliteCommand(stockSql, conn);
                    stockCmd.Parameters.AddWithValue("@id", _selectedMedicine.Id);
                    stockCmd.ExecuteNonQuery();
                }

                LogsPage.AddLog("Insert", "Returns",
                    $"Return saved - Voucher: {txtVoucherNo.Text}");

                MessageBox.Show("Return saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                txtError.Text = $"Error: {ex.Message}";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ReturnMedicine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}