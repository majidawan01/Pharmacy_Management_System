using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class SalaryPage : Page
    {
        public SalaryPage()
        {
            InitializeComponent();
            LoadMonths();
            LoadSalaries();
        }

        private void LoadMonths()
        {
            var months = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                months.Add(DateTime.Now.AddMonths(-i).ToString("yyyy-MM"));
            }
            cmbMonth.ItemsSource = months;
            cmbMonth.SelectedIndex = 0;
            txtCurrentMonth.Text = DateTime.Now.ToString("MMMM yyyy");
        }

        private void LoadSalaries(string search = "", string month = "")
        {
            try
            {
                var salaries = new List<SalaryRecord>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT s.*, e.Name as EmployeeName 
                               FROM Salaries s
                               JOIN Employees e ON s.EmployeeId = e.Id
                               WHERE e.Name LIKE @search";

                if (!string.IsNullOrEmpty(month))
                    sql += " AND s.Month LIKE @month";

                sql += " ORDER BY s.PaidDate DESC";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
                if (!string.IsNullOrEmpty(month))
                    cmd.Parameters.AddWithValue("@month", $"%{month}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    salaries.Add(new SalaryRecord
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                        EmployeeName = reader["EmployeeName"]?.ToString() ?? "",
                        Amount = Convert.ToDouble(reader["Amount"]),
                        Month = reader["Month"]?.ToString() ?? "",
                        PaidDate = reader["PaidDate"]?.ToString() ?? "",
                        Notes = reader["Notes"]?.ToString() ?? ""
                    });
                }

                dgSalaries.ItemsSource = salaries;
                txtTotalRecords.Text = salaries.Count.ToString();

                double total = 0;
                foreach (var s in salaries)
                    total += s.Amount;
                txtTotalPaid.Text = $"AFN {total:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSalaries(txtSearch.Text,
                cmbMonth.SelectedItem?.ToString() ?? "");
        }

        private void cmbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSalaries(txtSearch.Text,
                cmbMonth.SelectedItem?.ToString() ?? "");
        }

        private void btnPaySalary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddSalaryWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadSalaries();
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
                "Are you sure you want to delete this salary record?",
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
                        "DELETE FROM Salaries WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    LoadSalaries();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class SalaryRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public double Amount { get; set; }
        public string Month { get; set; } = "";
        public string PaidDate { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}