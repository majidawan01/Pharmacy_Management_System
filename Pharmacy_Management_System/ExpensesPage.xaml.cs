using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class ExpensesPage : Page
    {
        public ExpensesPage()
        {
            InitializeComponent();
            LoadExpenses();
        }

        private void LoadExpenses(string search = "", string date = "")
        {
            try
            {
                var expenses = new List<Expense>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Expenses 
                               WHERE (Title LIKE @search OR Category LIKE @search)";

                if (!string.IsNullOrEmpty(date))
                    sql += " AND ExpenseDate LIKE @date";

                sql += " ORDER BY ExpenseDate DESC";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
                if (!string.IsNullOrEmpty(date))
                    cmd.Parameters.AddWithValue("@date", $"%{date}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    expenses.Add(new Expense
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"]?.ToString() ?? "",
                        Category = reader["Category"]?.ToString() ?? "",
                        Amount = Convert.ToDouble(reader["Amount"]),
                        ExpenseDate = reader["ExpenseDate"]?.ToString() ?? "",
                        Notes = reader["Notes"]?.ToString() ?? ""
                    });
                }

                dgExpenses.ItemsSource = expenses;
                txtTotalExpenses.Text = expenses.Count.ToString();

                double total = 0;
                double todayTotal = 0;
                string today = DateTime.Today.ToString("yyyy-MM-dd");

                foreach (var exp in expenses)
                {
                    total += exp.Amount;
                    if (exp.ExpenseDate.StartsWith(today))
                        todayTotal += exp.Amount;
                }

                txtTotalAmount.Text = $"AFN {total:N0}";
                txtTodayExpenses.Text = $"AFN {todayTotal:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadExpenses(txtSearch.Text,
                dpFilter.SelectedDate?.ToString("yyyy-MM-dd") ?? "");
        }

        private void dpFilter_SelectedDateChanged(object sender,
            SelectionChangedEventArgs e)
        {
            LoadExpenses(txtSearch.Text,
                dpFilter.SelectedDate?.ToString("yyyy-MM-dd") ?? "");
        }

        private void btnAddExpense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddExpenseWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadExpenses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            int id = Convert.ToInt32(btn.Tag);
            try
            {
                var dialog = new AddExpenseWindow(id);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadExpenses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            int id = Convert.ToInt32(btn.Tag);

            var result = MessageBox.Show(
                "Are you sure you want to delete this expense?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var conn = Database.DatabaseHelper.GetConnection();
                    conn.Open();
                    using var cmd = new SqliteCommand(
                        "DELETE FROM Expenses WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    LoadExpenses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class Expense
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public double Amount { get; set; }
        public string ExpenseDate { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}