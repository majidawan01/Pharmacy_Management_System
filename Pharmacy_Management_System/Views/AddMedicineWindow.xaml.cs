using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace Pharmacy_Management_System.Views
{
    public partial class AddMedicineWindow : Window
    {
        private int _medicineId = 0;

        public AddMedicineWindow(int medicineId = 0)
        {
            InitializeComponent();
            _medicineId = medicineId;

            if (_medicineId > 0)
            {
                txtTitle.Text = "Edit Medicine";
                btnSave.Content = "Update Medicine";
                LoadMedicineData();
            }
        }

        private void LoadMedicineData()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                string sql = "SELECT * FROM Medicines WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", _medicineId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtCode.Text = reader["Code"]?.ToString() ?? "";
                    txtName.Text = reader["Name"]?.ToString() ?? "";
                    txtGenericName.Text = reader["GenericName"]?.ToString() ?? "";
                    txtCompany.Text = reader["Company"]?.ToString() ?? "";
                    txtCategory.Text = reader["Category"]?.ToString() ?? "";
                    txtUnit.Text = reader["Unit"]?.ToString() ?? "";
                    txtPurchasePrice.Text = reader["PurchasePrice"]?.ToString() ?? "0";
                    txtSalePrice.Text = reader["SalePrice"]?.ToString() ?? "0";
                    txtStock.Text = reader["Stock"]?.ToString() ?? "0";
                    txtMinStock.Text = reader["MinStock"]?.ToString() ?? "10";
                    if (DateTime.TryParse(
                        reader["ExpiryDate"]?.ToString(), out DateTime exp))
                        dpExpiryDate.SelectedDate = exp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtError.Text = "Medicine name is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (!double.TryParse(txtPurchasePrice.Text, out double purchasePrice))
                purchasePrice = 0;

            if (!double.TryParse(txtSalePrice.Text, out double salePrice))
                salePrice = 0;

            if (!int.TryParse(txtStock.Text, out int stock))
                stock = 0;

            if (!int.TryParse(txtMinStock.Text, out int minStock))
                minStock = 10;

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                // Disable foreign keys temporarily
                using var fkOff = new SqliteCommand(
                    "PRAGMA foreign_keys = OFF;", conn);
                fkOff.ExecuteNonQuery();

                // Auto-generate code if empty
                string code = txtCode.Text.Trim();
                if (string.IsNullOrEmpty(code))
                    code = $"MED-{DateTime.Now:yyyyMMddHHmmss}";

                // Check duplicate code for new medicine only
                if (_medicineId == 0)
                {
                    using var checkCmd = new SqliteCommand(
                        "SELECT COUNT(*) FROM Medicines WHERE Code=@code", conn);
                    checkCmd.Parameters.AddWithValue("@code", code);
                    long count = (long)checkCmd.ExecuteScalar()!;
                    if (count > 0)
                        code = $"{code}-{DateTime.Now:HHmmss}";
                }

                string sql;
                if (_medicineId == 0)
                {
                    sql = @"INSERT INTO Medicines 
                            (Code, Name, GenericName, Company, Category, Unit, 
                             PurchasePrice, SalePrice, Stock, MinStock, ExpiryDate)
                            VALUES 
                            (@code, @name, @genericName, @company, @category, @unit,
                             @purchasePrice, @salePrice, @stock, @minStock, @expiryDate)";
                }
                else
                {
                    sql = @"UPDATE Medicines SET
                            Code=@code, Name=@name, GenericName=@genericName,
                            Company=@company, Category=@category, Unit=@unit,
                            PurchasePrice=@purchasePrice, SalePrice=@salePrice,
                            Stock=@stock, MinStock=@minStock, ExpiryDate=@expiryDate
                            WHERE Id=@id";
                }

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@genericName",
                    string.IsNullOrEmpty(txtGenericName.Text.Trim())
                    ? "" : txtGenericName.Text.Trim());
                cmd.Parameters.AddWithValue("@company",
                    string.IsNullOrEmpty(txtCompany.Text.Trim())
                    ? "" : txtCompany.Text.Trim());
                cmd.Parameters.AddWithValue("@category",
                    string.IsNullOrEmpty(txtCategory.Text.Trim())
                    ? "" : txtCategory.Text.Trim());
                cmd.Parameters.AddWithValue("@unit",
                    string.IsNullOrEmpty(txtUnit.Text.Trim())
                    ? "pcs" : txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@purchasePrice", purchasePrice);
                cmd.Parameters.AddWithValue("@salePrice", salePrice);
                cmd.Parameters.AddWithValue("@stock", stock);
                cmd.Parameters.AddWithValue("@minStock", minStock);
                cmd.Parameters.AddWithValue("@expiryDate",
                    dpExpiryDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "");

                if (_medicineId > 0)
                    cmd.Parameters.AddWithValue("@id", _medicineId);

                cmd.ExecuteNonQuery();

                // Re-enable foreign keys
                using var fkOn = new SqliteCommand(
                    "PRAGMA foreign_keys = ON;", conn);
                fkOn.ExecuteNonQuery();

                if (_medicineId == 0)
                    LogsPage.AddLog("Insert", "Medicines",
                        $"New medicine added: {txtName.Text}");
                else
                    LogsPage.AddLog("Update", "Medicines",
                        $"Medicine updated: {txtName.Text}");

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