using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class PurchasePage : Page
    {
        public PurchasePage()
        {
            InitializeComponent();
            LoadPurchases();
        }

        private void LoadPurchases(string search = "")
        {
            try
            {
                var purchases = new List<Purchase>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Purchases 
                               WHERE SupplierName LIKE @search 
                               OR VoucherNo LIKE @search 
                               ORDER BY PurchaseDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    purchases.Add(new Purchase
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        VoucherNo = reader["VoucherNo"]?.ToString() ?? "",
                        SupplierName = reader["SupplierName"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                        PaidAmount = Convert.ToDouble(reader["PaidAmount"]),
                        RemainingAmount = Convert.ToDouble(reader["RemainingAmount"]),
                        PurchaseDate = reader["PurchaseDate"]?.ToString() ?? ""
                    });
                }

                dgPurchases.ItemsSource = purchases;
                txtTotalPurchases.Text = purchases.Count.ToString();

                double total = 0, pending = 0;
                foreach (var p in purchases)
                {
                    total += p.TotalAmount;
                    pending += p.RemainingAmount;
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
            LoadPurchases(txtSearch.Text);
        }

        private void btnAddPurchase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddPurchaseWindow();
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadPurchases();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.Tag == null) return;
                int id = Convert.ToInt32(btn.Tag);

                var dialog = new AddPurchaseWindow(id, viewMode: false);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadPurchases();
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
                    "Are you sure you want to delete this purchase?",
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
                        using var itemsCmd = new SqliteCommand(
                            "SELECT MedicineId, Quantity FROM PurchaseItems WHERE PurchaseId=@id",
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

                        foreach (var item in items)
                        {
                            using var stockCmd = new SqliteCommand(
                                "UPDATE Medicines SET Stock = Stock - @qty WHERE Id = @id",
                                conn, transaction);
                            stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                            stockCmd.Parameters.AddWithValue("@id", item.MedicineId);
                            stockCmd.ExecuteNonQuery();
                        }

                        using var deleteItems = new SqliteCommand(
                            "DELETE FROM PurchaseItems WHERE PurchaseId=@id",
                            conn, transaction);
                        deleteItems.Parameters.AddWithValue("@id", id);
                        deleteItems.ExecuteNonQuery();

                        using var deletePurchase = new SqliteCommand(
                            "DELETE FROM Purchases WHERE Id=@id",
                            conn, transaction);
                        deletePurchase.Parameters.AddWithValue("@id", id);
                        deletePurchase.ExecuteNonQuery();

                        transaction.Commit();

                        LogsPage.AddLog("Delete", "Purchases",
                            $"Purchase deleted - ID: {id}");

                        MessageBox.Show("Purchase deleted successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadPurchases();
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

    public class Purchase
    {
        public int Id { get; set; }
        public string VoucherNo { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public double TotalAmount { get; set; }
        public double PaidAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string PurchaseDate { get; set; } = "";
    }
}