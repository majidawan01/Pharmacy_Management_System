using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class AddAccountWindow : Window
    {
        private int _accountId = 0;
        private bool _isPayment = false;

        public AddAccountWindow(int accountId = 0, bool isPayment = false)
        {
            InitializeComponent();
            _accountId = accountId;
            _isPayment = isPayment;

            if (_isPayment && _accountId > 0)
            {
                txtTitle.Text = "Record Payment";
                btnSave.Content = "💳 Record Payment";
                btnSave.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(46, 125, 50));
                LoadAccountForPayment();
            }
        }

        private void LoadAccountForPayment()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT * FROM Accounts WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _accountId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtCompanyName.Text = reader["CompanyName"]?.ToString() ?? "";
                    txtCompanyName.IsReadOnly = true;
                    txtTotalAmount.Text = reader["TotalAmount"]?.ToString() ?? "0";
                    txtTotalAmount.IsReadOnly = true;
                    txtPaidAmount.Text = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                txtError.Text = "Company name is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (!double.TryParse(txtTotalAmount.Text, out double total) || total <= 0)
            {
                txtError.Text = "Please enter valid total amount!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            double.TryParse(txtPaidAmount.Text, out double paid);
            string type = (cmbAccountType.SelectedItem as ComboBoxItem)?.Content?.ToString()
                ?? "Receivable";

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                if (_isPayment && _accountId > 0)
                {
                    string sql = @"UPDATE Accounts SET 
                        PaidAmount = PaidAmount + @paid,
                        RemainingAmount = RemainingAmount - @paid
                        WHERE Id = @id";
                    using var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@paid", paid);
                    cmd.Parameters.AddWithValue("@id", _accountId);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    double remaining = total - paid;
                    string sql = @"INSERT INTO Accounts 
                        (CompanyName, AccountType, TotalAmount, PaidAmount, 
                         RemainingAmount, Notes)
                        VALUES (@name, @type, @total, @paid, @remaining, @notes)";
                    using var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", txtCompanyName.Text.Trim());
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@paid", paid);
                    cmd.Parameters.AddWithValue("@remaining", remaining);
                    cmd.Parameters.AddWithValue("@notes", txtNotes.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

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
}