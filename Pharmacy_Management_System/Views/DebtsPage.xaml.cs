using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class DebtsPage : Page
    {
        public DebtsPage()
        {
            InitializeComponent();
            LoadDebts();
        }

        private void LoadDebts(string search = "")
        {
            try
            {
                var debts = new List<Debt>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Debts 
                               WHERE CustomerName LIKE @search 
                               OR Phone LIKE @search
                               ORDER BY DebtDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    debts.Add(new Debt
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CustomerName = reader["CustomerName"]?.ToString() ?? "",
                        Phone = reader["Phone"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                        PaidAmount = Convert.ToDouble(reader["PaidAmount"]),
                        RemainingAmount = Convert.ToDouble(reader["RemainingAmount"]),
                        DebtDate = reader["DebtDate"]?.ToString() ?? "",
                        Notes = reader["Notes"]?.ToString() ?? ""
                    });
                }

                dgDebts.ItemsSource = debts;
                txtTotalDebts.Text = debts.Count.ToString();

                double total = 0, remaining = 0;
                foreach (var d in debts)
                {
                    total += d.TotalAmount;
                    remaining += d.RemainingAmount;
                }
                txtTotalAmount.Text = $"AFN {total:N0}";
                txtRemainingAmount.Text = $"AFN {remaining:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDebts(txtSearch.Text);
        }

        private void btnAddDebt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddDebtWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadDebts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            int id = Convert.ToInt32(btn.Tag);

            try
            {
                var dialog = new AddDebtWindow(id, isPayment: true);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadDebts();
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
                "Are you sure you want to delete this debt record?",
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
                        "DELETE FROM Debts WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    LoadDebts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class Debt
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public double TotalAmount { get; set; }
        public double PaidAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string DebtDate { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}