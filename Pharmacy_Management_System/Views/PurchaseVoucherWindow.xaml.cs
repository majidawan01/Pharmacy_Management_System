using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class PurchaseVoucherWindow : Window
    {
        private int _purchaseId = 0;

        public PurchaseVoucherWindow(int purchaseId)
        {
            InitializeComponent();
            _purchaseId = purchaseId;
            this.Loaded += (s, e) => LoadVoucherData();
        }

        private void LoadVoucherData()
        {
            try
            {
                LoadPharmacyInfo();

                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                using var cmd = new SqliteCommand(
                    "SELECT * FROM Purchases WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _purchaseId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtVoucherNo.Text = reader["VoucherNo"]?.ToString() ?? "";
                    txtSupplier.Text = reader["SupplierName"]?.ToString() ?? "";
                    txtDate.Text = reader["PurchaseDate"]?.ToString() ?? "";
                    txtTotal.Text = $"AFN {Convert.ToDouble(reader["TotalAmount"]):N0}";
                    txtPaid.Text = $"AFN {Convert.ToDouble(reader["PaidAmount"]):N0}";
                    txtRemaining.Text = $"AFN {Convert.ToDouble(reader["RemainingAmount"]):N0}";
                }
                reader.Close();

                var items = new List<PurchaseVoucherItem>();
                using var itemCmd = new SqliteCommand(
                    @"SELECT pi.Quantity, pi.PurchasePrice, pi.TotalPrice, m.Name
                      FROM PurchaseItems pi
                      JOIN Medicines m ON pi.MedicineId = m.Id
                      WHERE pi.PurchaseId = @id", conn);
                itemCmd.Parameters.AddWithValue("@id", _purchaseId);
                using var itemReader = itemCmd.ExecuteReader();

                while (itemReader.Read())
                {
                    items.Add(new PurchaseVoucherItem
                    {
                        MedicineName = itemReader["Name"]?.ToString() ?? "",
                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                        Price = Convert.ToDouble(itemReader["PurchasePrice"]),
                        Total = Convert.ToDouble(itemReader["TotalPrice"])
                    });
                }

                itemsList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
                        //case "Phone":
                        //    if (!string.IsNullOrEmpty(val))
                        //        txtPharmacyPhone.Text = $"Phone: {val}";
                        //    break;
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

                    var originalTransform = pnlVoucher.LayoutTransform;
                    pnlVoucher.LayoutTransform =
                        System.Windows.Media.Transform.Identity;

                    pnlVoucher.Measure(new System.Windows.Size(
                        pageW, double.PositiveInfinity));

                    double naturalW = pnlVoucher.DesiredSize.Width;
                    double naturalH = pnlVoucher.DesiredSize.Height;

                    double scaleX = pageW / naturalW;
                    double scaleY = pageH / naturalH;
                    double scale = Math.Min(scaleX, scaleY);

                    if (scale > 1.0) scale = 1.0;

                    pnlVoucher.LayoutTransform =
                        new System.Windows.Media.ScaleTransform(scale, scale);

                    pnlVoucher.Measure(
                        new System.Windows.Size(pageW, pageH));
                    pnlVoucher.Arrange(
                        new Rect(new System.Windows.Size(pageW, pageH)));

                    printDialog.PrintVisual(pnlVoucher, "Purchase Voucher");

                    pnlVoucher.LayoutTransform = originalTransform;
                    pnlVoucher.Measure(new System.Windows.Size(
                        double.PositiveInfinity, double.PositiveInfinity));
                    pnlVoucher.Arrange(new Rect(pnlVoucher.DesiredSize));

                    MessageBox.Show("Voucher printed successfully!",
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

    public class PurchaseVoucherItem
    {
        public string MedicineName { get; set; } = "";
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
    }
}