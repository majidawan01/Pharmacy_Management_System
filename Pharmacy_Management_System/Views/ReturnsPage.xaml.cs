using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class ReturnsPage : Page
    {
        public ReturnsPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadReturns();
        }

        private void LoadReturns(string search = "", string type = "")
        {
            try
            {
                var returns = new List<Return>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Returns 
                               WHERE PartyName LIKE @search";

                if (!string.IsNullOrEmpty(type) && type != "All")
                    sql += " AND Type = @type";

                sql += " ORDER BY ReturnDate DESC";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
                if (!string.IsNullOrEmpty(type) && type != "All")
                    cmd.Parameters.AddWithValue("@type", type);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    returns.Add(new Return
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Type = reader["Type"]?.ToString() ?? "",
                        VoucherNo = reader["VoucherNo"]?.ToString() ?? "",
                        PartyName = reader["PartyName"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                        ReturnDate = reader["ReturnDate"]?.ToString() ?? "",
                        Notes = reader["Notes"]?.ToString() ?? ""
                    });
                }

                dgReturns.ItemsSource = returns;
                txtTotalReturns.Text = returns.Count.ToString();
                txtCustomerReturns.Text = returns.FindAll(
                    r => r.Type == "Customer Return").Count.ToString();
                txtCompanyReturns.Text = returns.FindAll(
                    r => r.Type == "Company Return").Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (cmbTypeFilter == null || dgReturns == null) return;
                string type = (cmbTypeFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                LoadReturns(txtSearch?.Text ?? "", type);
            }
            catch { }
        }

        private void cmbTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTypeFilter == null || dgReturns == null) return;
                string type = (cmbTypeFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                LoadReturns(txtSearch?.Text ?? "", type);
            }
            catch { }
        }

        private void btnAddReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddReturnWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadReturns();
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
                "Are you sure you want to delete this return record?",
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
                        "DELETE FROM Returns WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    LoadReturns();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class Return
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string VoucherNo { get; set; } = "";
        public string PartyName { get; set; } = "";
        public double TotalAmount { get; set; }
        public string ReturnDate { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}