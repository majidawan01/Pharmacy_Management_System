using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class SalesPage : Page
    {
        public SalesPage()
        {
            InitializeComponent();
            LoadSales();
        }

        private void LoadSales(string search = "")
        {
            try
            {
                var sales = new List<Sale>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Sales 
                               WHERE CustomerName LIKE @search 
                               OR VoucherNo LIKE @search 
                               ORDER BY SaleDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sales.Add(new Sale
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        VoucherNo = reader["VoucherNo"]?.ToString() ?? "",
                        CustomerName = reader["CustomerName"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                        PaidAmount = Convert.ToDouble(reader["PaidAmount"]),
                        RemainingAmount = Convert.ToDouble(reader["RemainingAmount"]),
                        SaleDate = reader["SaleDate"]?.ToString() ?? ""
                    });
                }

                dgSales.ItemsSource = sales;
                txtTotalSales.Text = sales.Count.ToString();

                double total = 0, pending = 0;
                foreach (var s in sales)
                {
                    total += s.TotalAmount;
                    pending += s.RemainingAmount;
                }
                txtTotalAmount.Text = $"AFN {total:N0}";
                txtPendingAmount.Text = $"AFN {pending:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSales(txtSearch.Text);
        }

        private void btnAddSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddSaleWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadSales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.Tag == null) return;
                int saleId = Convert.ToInt32(btn.Tag);

                var billWindow = new BillPrintWindow(saleId);
                billWindow.Owner = Window.GetWindow(this);
                billWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.Tag == null) return;
                int id = Convert.ToInt32(btn.Tag);

                var result = MessageBox.Show(
                    "Are you sure you want to delete this sale?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using var conn = Database.DatabaseHelper.GetConnection();
                    conn.Open();
                    using var transaction = conn.BeginTransaction();

                    try
                    {
                        // Restore stock first
                        using var itemsCmd = new SqliteCommand(
                            "SELECT MedicineId, Quantity FROM SaleItems WHERE SaleId=@id",
                            conn, transaction);
                        itemsCmd.Parameters.AddWithValue("@id", id);
                        using var reader = itemsCmd.ExecuteReader();

                        var items = new List<(int MedicineId, int Quantity)>();
                        while (reader.Read())
                        {
                            items.Add((
                                Convert.ToInt32(reader["MedicineId"]),
                                Convert.ToInt32(reader["Quantity"])
                            ));
                        }
                        reader.Close();

                        // Restore stock
                        foreach (var item in items)
                        {
                            using var stockCmd = new SqliteCommand(
                                "UPDATE Medicines SET Stock = Stock + @qty WHERE Id = @id",
                                conn, transaction);
                            stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                            stockCmd.Parameters.AddWithValue("@id", item.MedicineId);
                            stockCmd.ExecuteNonQuery();
                        }

                        // Delete sale items first
                        using var deleteItems = new SqliteCommand(
                            "DELETE FROM SaleItems WHERE SaleId=@id",
                            conn, transaction);
                        deleteItems.Parameters.AddWithValue("@id", id);
                        deleteItems.ExecuteNonQuery();

                        // Then delete sale
                        using var deleteSale = new SqliteCommand(
                            "DELETE FROM Sales WHERE Id=@id",
                            conn, transaction);
                        deleteSale.Parameters.AddWithValue("@id", id);
                        deleteSale.ExecuteNonQuery();

                        transaction.Commit();

                        LogsPage.AddLog("Delete", "Sales",
                            $"Sale deleted - ID: {id}");

                        MessageBox.Show("Sale deleted successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadSales();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
        public class Sale
    {
        public int Id { get; set; }
        public string VoucherNo { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public double TotalAmount { get; set; }
        public double PaidAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string SaleDate { get; set; } = "";
    }
}