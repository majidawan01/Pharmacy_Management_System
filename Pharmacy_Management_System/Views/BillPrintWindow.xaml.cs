using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class BillPrintWindow : Window
    {
        private int _saleId = 0;

        public BillPrintWindow(int saleId)
        {
            InitializeComponent();
            _saleId = saleId;
            this.Loaded += (s, e) => LoadBillData();
        }

        private void LoadBillData()
        {
            try
            {
                LoadPharmacyInfo();

                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                using var cmd = new SqliteCommand(
                    "SELECT * FROM Sales WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _saleId);
                using var reader = cmd.ExecuteReader();

                double total = 0, paid = 0, remaining = 0;

                if (reader.Read())
                {
                    txtVoucherNo.Text = reader["VoucherNo"]?.ToString() ?? "";
                    txtCustomer.Text = reader["CustomerName"]?.ToString() ?? "Walk-in Customer";
                    txtDate.Text = reader["SaleDate"]?.ToString() ?? "";
                    total = Convert.ToDouble(reader["TotalAmount"]);
                    paid = Convert.ToDouble(reader["PaidAmount"]);
                    remaining = Convert.ToDouble(reader["RemainingAmount"]);

                    txtSubTotal.Text = $"AFN {total:N0}";
                    txtDiscount.Text = "AFN 0";
                    txtTotal.Text = $"AFN {total:N0}";
                    txtPaid.Text = $"AFN {paid:N0}";
                   // txtRemaining.Text = $"AFN {remaining:N0}";
                }
                reader.Close();

                var items = new List<BillItem>();
                using var itemCmd = new SqliteCommand(
                    @"SELECT si.Quantity, si.SalePrice, si.TotalPrice, m.Name
                      FROM SaleItems si
                      JOIN Medicines m ON si.MedicineId = m.Id
                      WHERE si.SaleId = @id", conn);
                itemCmd.Parameters.AddWithValue("@id", _saleId);
                using var itemReader = itemCmd.ExecuteReader();

                while (itemReader.Read())
                {
                    items.Add(new BillItem
                    {
                        MedicineName = itemReader["Name"]?.ToString() ?? "",
                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                        Price = Convert.ToDouble(itemReader["SalePrice"]),
                        Total = Convert.ToDouble(itemReader["TotalPrice"])
                    });
                }

                itemsList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bill: {ex.Message}");
            }
        }

        private void LoadPharmacyInfo()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                using var checkCmd = new SqliteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name='Settings'",
                    conn);
                var exists = checkCmd.ExecuteScalar();
                if (exists == null) return;

                using var cmd = new SqliteCommand(
                    "SELECT Key, Value FROM Settings", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string key = reader["Key"]?.ToString() ?? "";
                    string val = reader["Value"]?.ToString() ?? "";
                    switch (key)
                    {
                        case "PharmacyName":
                            if (!string.IsNullOrEmpty(val))
                                txtPharmacyName.Text = val.ToUpper();
                            break;
                        case "Address":
                            if (!string.IsNullOrEmpty(val))
                                txtPharmacyAddress.Text = val;
                            break;
                        case "Email":
                            if (!string.IsNullOrEmpty(val))
                                txtPharmacyPhone.Text = val;
                            break;
                    }
                }
            }
            catch { }
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();

                // A4 Portrait
                printDialog.PrintTicket.PageOrientation =
                    System.Printing.PageOrientation.Portrait;
                printDialog.PrintTicket.PageMediaSize =
                    new System.Printing.PageMediaSize(
                        System.Printing.PageMediaSizeName.ISOA4);

                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintTicket.PageOrientation =
                        System.Printing.PageOrientation.Portrait;

                    double pageW = printDialog.PrintableAreaWidth;
                    double pageH = printDialog.PrintableAreaHeight;

                    var originalTransform = pnlBill.LayoutTransform;
                    pnlBill.LayoutTransform =
                        System.Windows.Media.Transform.Identity;

                    pnlBill.Measure(new System.Windows.Size(
                        pageW, double.PositiveInfinity));

                    double naturalW = pnlBill.DesiredSize.Width;
                    double naturalH = pnlBill.DesiredSize.Height;

                    double scaleX = pageW / naturalW;
                    double scaleY = pageH / naturalH;
                    double scale = Math.Min(scaleX, scaleY);

                    if (scale > 1.0) scale = 1.0;

                    pnlBill.LayoutTransform =
                        new System.Windows.Media.ScaleTransform(scale, scale);

                    pnlBill.Measure(new System.Windows.Size(pageW, pageH));
                    pnlBill.Arrange(new Rect(
                        new System.Windows.Size(pageW, pageH)));

                    printDialog.PrintVisual(pnlBill, "Sale Invoice");

                    pnlBill.LayoutTransform = originalTransform;
                    pnlBill.Measure(new System.Windows.Size(
                        double.PositiveInfinity, double.PositiveInfinity));
                    pnlBill.Arrange(new Rect(pnlBill.DesiredSize));

                    MessageBox.Show("Invoice printed successfully!",
                        "Success", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print error: {ex.Message}");
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class BillItem
    {
        public string MedicineName { get; set; } = "";
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
    }
}