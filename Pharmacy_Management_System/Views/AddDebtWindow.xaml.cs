using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace Pharmacy_Management_System.Views
{
    public partial class AddDebtWindow : Window
    {
        private int _debtId = 0;
        private bool _isPayment = false;

        public AddDebtWindow(int debtId = 0, bool isPayment = false)
        {
            InitializeComponent();
            _debtId = debtId;
            _isPayment = isPayment;

            if (_isPayment && _debtId > 0)
            {
                txtTitle.Text = "Record Payment";
                txtSubTitle.Text = "Enter payment amount for this debt";
                btnSave.Content = "💳 Record Payment";
                btnSave.Background =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(46, 125, 50));
                LoadDebtForPayment();
            }
        }

        private void LoadDebtForPayment()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT * FROM Debts WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _debtId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtCustomerName.Text = reader["CustomerName"]?.ToString() ?? "";
                    txtCustomerName.IsReadOnly = true;
                    txtPhone.Text = reader["Phone"]?.ToString() ?? "";
                    txtPhone.IsReadOnly = true;
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
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                txtError.Text = "Customer name is required!";
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

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                if (_isPayment && _debtId > 0)
                {
                    // Update existing debt payment
                    string sql = @"UPDATE Debts SET 
                        PaidAmount = PaidAmount + @paid,
                        RemainingAmount = RemainingAmount - @paid
                        WHERE Id = @id";
                    using var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@paid", paid);
                    cmd.Parameters.AddWithValue("@id", _debtId);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // New debt
                    double remaining = total - paid;
                    string sql = @"INSERT INTO Debts 
                        (CustomerName, Phone, TotalAmount, PaidAmount, 
                         RemainingAmount, Notes)
                        VALUES (@name, @phone, @total, @paid, @remaining, @notes)";
                    using var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", txtCustomerName.Text.Trim());
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
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