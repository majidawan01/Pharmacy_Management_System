using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace Pharmacy_Management_System.Views
{
    public partial class AddExpenseWindow : Window
    {
        private int _expenseId = 0;

        public AddExpenseWindow(int expenseId = 0)
        {
            InitializeComponent();
            _expenseId = expenseId;
            dpExpenseDate.SelectedDate = DateTime.Today;

            if (_expenseId > 0)
            {
                txtTitle.Text = "Edit Expense";
                btnSave.Content = "Update Expense";
                LoadExpenseData();
            }
        }

        private void LoadExpenseData()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT * FROM Expenses WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _expenseId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtExpenseTitle.Text = reader["Title"]?.ToString() ?? "";
                    txtAmount.Text = reader["Amount"]?.ToString() ?? "0";
                    txtNotes.Text = reader["Notes"]?.ToString() ?? "";

                    string cat = reader["Category"]?.ToString() ?? "";
                    foreach (System.Windows.Controls.ComboBoxItem item in cmbCategory.Items)
                    {
                        if (item.Content?.ToString() == cat)
                        {
                            cmbCategory.SelectedItem = item;
                            break;
                        }
                    }

                    if (DateTime.TryParse(reader["ExpenseDate"]?.ToString(),
                        out DateTime d))
                        dpExpenseDate.SelectedDate = d;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExpenseTitle.Text))
            {
                txtError.Text = "Expense title is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (!double.TryParse(txtAmount.Text, out double amount) || amount <= 0)
            {
                txtError.Text = "Please enter valid amount!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            string category = (cmbCategory.SelectedItem as
                System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Other";

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql;
                if (_expenseId == 0)
                {
                    sql = @"INSERT INTO Expenses (Title, Category, Amount, ExpenseDate, Notes)
                            VALUES (@title, @category, @amount, @date, @notes)";
                }
                else
                {
                    sql = @"UPDATE Expenses SET Title=@title, Category=@category,
                            Amount=@amount, ExpenseDate=@date, Notes=@notes
                            WHERE Id=@id";
                }

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@title", txtExpenseTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@date",
                    dpExpenseDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                    DateTime.Today.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@notes", txtNotes.Text.Trim());
                if (_expenseId > 0)
                    cmd.Parameters.AddWithValue("@id", _expenseId);

                cmd.ExecuteNonQuery();
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