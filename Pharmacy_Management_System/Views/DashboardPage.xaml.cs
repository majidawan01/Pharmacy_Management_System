using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                LoadDashboardData();
                LoadCharts();
            };
        }

        private void LoadDashboardData()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string today = DateTime.Today.ToString("yyyy-MM-dd");

                // Today Sales
                using var salesCmd = new SqliteCommand(
                    "SELECT COALESCE(SUM(TotalAmount),0) FROM Sales WHERE date(SaleDate)=@today", conn);
                salesCmd.Parameters.AddWithValue("@today", today);
                txtTodaySales.Text = $"AFN {Convert.ToDouble(salesCmd.ExecuteScalar()):N0}";

                // Today Purchase
                using var purCmd = new SqliteCommand(
                    "SELECT COALESCE(SUM(TotalAmount),0) FROM Purchases WHERE date(PurchaseDate)=@today", conn);
                purCmd.Parameters.AddWithValue("@today", today);
                txtTodayPurchase.Text = $"AFN {Convert.ToDouble(purCmd.ExecuteScalar()):N0}";

                // Stock Value
                using var stockCmd = new SqliteCommand(
                    "SELECT COALESCE(SUM(Stock * PurchasePrice),0) FROM Medicines", conn);
                txtStockValue.Text = $"AFN {Convert.ToDouble(stockCmd.ExecuteScalar()):N0}";

                // Low Stock
                using var lowCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Medicines WHERE Stock <= MinStock", conn);
                txtLowStock.Text = $"{lowCmd.ExecuteScalar()} Items";

                // Near Expiry
                string expiryDate = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");
                using var nearCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Medicines WHERE ExpiryDate != '' AND date(ExpiryDate) <= @exp AND date(ExpiryDate) >= @today", conn);
                nearCmd.Parameters.AddWithValue("@exp", expiryDate);
                nearCmd.Parameters.AddWithValue("@today", today);
                txtNearExpiry.Text = $"{nearCmd.ExecuteScalar()} Items";

                // Expired
                using var expCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Medicines WHERE ExpiryDate != '' AND date(ExpiryDate) < @today", conn);
                expCmd.Parameters.AddWithValue("@today", today);
                txtExpired.Text = $"{expCmd.ExecuteScalar()} Items";

                // Total Medicines
                using var medCmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Medicines", conn);
                txtTotalMedicines.Text = medCmd.ExecuteScalar()?.ToString() ?? "0";

                // Recent Sales
                var sales = new List<Sale>();
                using var recentCmd = new SqliteCommand(
                    "SELECT VoucherNo, CustomerName, TotalAmount, SaleDate FROM Sales ORDER BY SaleDate DESC LIMIT 10", conn);
                using var reader = recentCmd.ExecuteReader();
                while (reader.Read())
                {
                    sales.Add(new Sale
                    {
                        VoucherNo = reader["VoucherNo"]?.ToString() ?? "",
                        CustomerName = reader["CustomerName"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                        SaleDate = reader["SaleDate"]?.ToString() ?? ""
                    });
                }
                dgRecentSales.ItemsSource = sales;

                // Low Stock Alert
                using var lowAlertCmd = new SqliteCommand(
                    "SELECT Name, Stock FROM Medicines WHERE Stock <= MinStock LIMIT 5", conn);
                using var alertReader = lowAlertCmd.ExecuteReader();
                string lowStockText = "";
                while (alertReader.Read())
                    lowStockText += $"• {alertReader["Name"]} ({alertReader["Stock"]} left)\n";
                txtLowStockAlert.Text = string.IsNullOrEmpty(lowStockText)
                    ? "No low stock items" : lowStockText.TrimEnd();

                // Expiry Alert
                using var expiryAlertCmd = new SqliteCommand(
                    "SELECT Name, ExpiryDate FROM Medicines WHERE ExpiryDate != '' AND date(ExpiryDate) <= @exp AND date(ExpiryDate) >= @today LIMIT 5", conn);
                expiryAlertCmd.Parameters.AddWithValue("@exp", expiryDate);
                expiryAlertCmd.Parameters.AddWithValue("@today", today);
                using var expiryReader = expiryAlertCmd.ExecuteReader();
                string expiryText = "";
                while (expiryReader.Read())
                    expiryText += $"• {expiryReader["Name"]} (Exp: {expiryReader["ExpiryDate"]})\n";
                txtExpiryAlert.Text = string.IsNullOrEmpty(expiryText)
                    ? "No expiring items" : expiryText.TrimEnd();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Dashboard Error: {ex.Message}");
            }
        }

        private void LoadCharts()
        {
            try
            {
                var salesValues = new ChartValues<double>();
                var purchaseValues = new ChartValues<double>();
                var labels = new List<string>();

                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                // Last 7 days data
                for (int i = 6; i >= 0; i--)
                {
                    string date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                    string label = DateTime.Today.AddDays(-i).ToString("MM/dd");
                    labels.Add(label);

                    // Sales
                    using var sCmd = new SqliteCommand(
                        "SELECT COALESCE(SUM(TotalAmount),0) FROM Sales WHERE date(SaleDate)=@date", conn);
                    sCmd.Parameters.AddWithValue("@date", date);
                    salesValues.Add(Convert.ToDouble(sCmd.ExecuteScalar()));

                    // Purchase
                    using var pCmd = new SqliteCommand(
                        "SELECT COALESCE(SUM(TotalAmount),0) FROM Purchases WHERE date(PurchaseDate)=@date", conn);
                    pCmd.Parameters.AddWithValue("@date", date);
                    purchaseValues.Add(Convert.ToDouble(pCmd.ExecuteScalar()));
                }

                // Sales Chart
                salesChart.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Sales",
                        Values = salesValues,
                        Fill = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(21, 101, 192)),
                        StrokeThickness = 0
                    }
                };
                salesAxisX.Labels = labels;
                // Sales Chart Y-Axis format fix
                salesChart.AxisY[0].LabelFormatter = value => value.ToString("N0");

                // Purchase Chart Y-Axis format fix  
                purchaseChart.AxisY[0].LabelFormatter = value => value.ToString("N0");
                // Purchase Chart
                purchaseChart.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Purchase",
                        Values = purchaseValues,
                        Fill = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(2, 136, 209)),
                        StrokeThickness = 0
                    }
                };
                purchaseAxisX.Labels = labels;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Chart Error: {ex.Message}");
            }
        }
    }
}