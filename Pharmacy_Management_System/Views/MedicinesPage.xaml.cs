using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class MedicinesPage : Page
    {
        public MedicinesPage()
        {
            InitializeComponent();
            LoadMedicines();
        }

        private void LoadMedicines(string search = "")
        {
            try
            {
                var medicines = new List<Medicine>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = "SELECT * FROM Medicines WHERE Name LIKE @search OR Code LIKE @search OR GenericName LIKE @search ORDER BY Name";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    medicines.Add(new Medicine
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Code = reader["Code"]?.ToString() ?? "",
                        Name = reader["Name"]?.ToString() ?? "",
                        GenericName = reader["GenericName"]?.ToString() ?? "",
                        Company = reader["Company"]?.ToString() ?? "",
                        PurchasePrice = Convert.ToDouble(reader["PurchasePrice"]),
                        SalePrice = Convert.ToDouble(reader["SalePrice"]),
                        Stock = Convert.ToInt32(reader["Stock"]),
                        ExpiryDate = reader["ExpiryDate"]?.ToString() ?? ""
                    });
                }

                dgMedicines.ItemsSource = medicines;
                txtTotalCount.Text = medicines.Count.ToString();
                txtLowStockCount.Text = medicines.FindAll(m => m.Stock <= 10).Count.ToString();
                txtExpiredCount.Text = medicines.FindAll(m =>
                    !string.IsNullOrEmpty(m.ExpiryDate) &&
                    DateTime.TryParse(m.ExpiryDate, out DateTime exp) &&
                    exp < DateTime.Now).Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMedicines(txtSearch.Text);
        }

        private void btnAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddMedicineWindow();
            if (dialog.ShowDialog() == true)
                LoadMedicines();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.Tag == null) return;
                int id = Convert.ToInt32(btn.Tag);
                var dialog = new AddMedicineWindow(id);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    LogsPage.AddLog("Update", "Medicines", $"Medicine updated - ID: {id}");
                    LoadMedicines();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening edit: {ex.Message}");
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
                    "Are you sure you want to delete this medicine?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using var conn = Database.DatabaseHelper.GetConnection();
                    conn.Open();

                    // Pehle check karo koi sale ya purchase record hai
                    using var checkCmd = new SqliteCommand(
                        @"SELECT COUNT(*) FROM SaleItems WHERE MedicineId=@id", conn);
                    checkCmd.Parameters.AddWithValue("@id", id);
                    long saleCount = (long)checkCmd.ExecuteScalar()!;

                    using var checkCmd2 = new SqliteCommand(
                        @"SELECT COUNT(*) FROM PurchaseItems WHERE MedicineId=@id", conn);
                    checkCmd2.Parameters.AddWithValue("@id", id);
                    long purCount = (long)checkCmd2.ExecuteScalar()!;

                    if (saleCount > 0 || purCount > 0)
                    {
                        MessageBox.Show(
                            $"This medicine cannot be deleted!\n\n" +
                            $"It has {saleCount} sale record(s) and {purCount} purchase record(s).\n\n" +
                            "Delete related sales/purchases first.",
                            "Cannot Delete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Safe to delete
                    using var cmd = new SqliteCommand(
                        "DELETE FROM Medicines WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();

                    LogsPage.AddLog("Delete", "Medicines",
                        $"Medicine deleted - ID: {id}");

                    MessageBox.Show("Medicine deleted successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMedicines();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }

        public class Medicine
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string GenericName { get; set; } = "";
        public string Company { get; set; } = "";
        public double PurchasePrice { get; set; }
        public double SalePrice { get; set; }
        public int Stock { get; set; }
        public string ExpiryDate { get; set; } = "";
    }
}