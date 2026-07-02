using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pharmacy_Management_System.Views
{
    public partial class AddSaleWindow : Window
    {
        private List<SaleItem> _items = new();
        private List<MedicineStock> _medicines = new();
        private MedicineStock? _selectedMedicine = null;
        private bool _isSaved = false;

        public AddSaleWindow()
        {
            InitializeComponent();
            txtVoucherNo.Text = $"SAL-{DateTime.Now:yyyyMMdd-HHmmss}";
            dpSaleDate.SelectedDate = DateTime.Today;
            this.Loaded += (s, e) =>
            {
                LoadMedicines();
                txtMedicineSearch.Focus();
            };
        }

        private void LoadMedicines()
        {
            try
            {
                _medicines = new List<MedicineStock>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT Id, Name, SalePrice, Stock FROM Medicines ORDER BY Name",
                    conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _medicines.Add(new MedicineStock
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        SalePrice = reader.GetDouble(2),
                        Stock = reader.GetInt32(3)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading medicines: {ex.Message}");
            }
        }

        private void txtMedicineSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string search = txtMedicineSearch.Text.Trim().ToLower();

                if (string.IsNullOrEmpty(search))
                {
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                    _selectedMedicine = null;
                    return;
                }

                var filtered = _medicines
                    .Where(m => m.Name.ToLower().Contains(search))
                    .Take(10)
                    .ToList();

                if (filtered.Count > 0)
                {
                    lstMedicineSuggestions.ItemsSource = filtered
                        .Select(m => $"{m.Name}  (Stock: {m.Stock})")
                        .ToList();
                    lstMedicineSuggestions.Visibility = Visibility.Visible;
                }
                else
                {
                    lstMedicineSuggestions.ItemsSource = null;
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void txtMedicineSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    if (lstMedicineSuggestions.Items.Count > 0)
                    {
                        lstMedicineSuggestions.Focus();
                        lstMedicineSuggestions.SelectedIndex = 0;
                    }
                }
                else if (e.Key == Key.Enter)
                {
                    if (lstMedicineSuggestions.Items.Count == 1)
                    {
                        lstMedicineSuggestions.SelectedIndex = 0;
                        SelectMedicine();
                    }
                    else if (lstMedicineSuggestions.Items.Count > 1)
                    {
                        lstMedicineSuggestions.Focus();
                        lstMedicineSuggestions.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private void lstMedicineSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SelectMedicine();
                }
                else if (e.Key == Key.Escape)
                {
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                    txtMedicineSearch.Focus();
                }
            }
            catch { }
        }

        private void lstMedicineSuggestions_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            try
            {
                if (lstMedicineSuggestions.SelectedItem == null) return;

                string selectedText = lstMedicineSuggestions.SelectedItem.ToString() ?? "";
                string medicineName = selectedText.Split(
                    new[] { "  (Stock:" }, StringSplitOptions.None)[0].Trim();

                _selectedMedicine = _medicines.FirstOrDefault(m => m.Name == medicineName);

                if (_selectedMedicine != null &&
                    e.AddedItems.Count > 0 &&
                    Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    SelectMedicine();
                }
            }
            catch { }
        }

        private void SelectMedicine()
        {
            try
            {
                if (lstMedicineSuggestions.SelectedItem == null) return;

                string selectedText = lstMedicineSuggestions.SelectedItem.ToString() ?? "";
                string medicineName = selectedText.Split(
                    new[] { "  (Stock:" }, StringSplitOptions.None)[0].Trim();

                _selectedMedicine = _medicines.FirstOrDefault(m => m.Name == medicineName);

                if (_selectedMedicine != null)
                {
                    txtMedicineSearch.Text = _selectedMedicine.Name;
                    txtItemPrice.Text = _selectedMedicine.SalePrice.ToString();
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                    txtQty.Focus();
                    txtQty.SelectAll();
                }
            }
            catch { }
        }

        private void txtQty_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    txtItemPrice.Focus();
                    txtItemPrice.SelectAll();
                }
            }
            catch { }
        }

        private void txtItemPrice_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    AddItemToList();
                }
            }
            catch { }
        }

        private void txtQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateItemTotal();
        }

        private void txtItemPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateItemTotal();
        }

        private void UpdateItemTotal()
        {
            try
            {
                if (double.TryParse(txtItemPrice.Text, out double price) &&
                    int.TryParse(txtQty.Text, out int qty))
                {
                    txtItemTotal.Text = (price * qty).ToString("N0");
                }
            }
            catch { }
        }

        private void txtPaidAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            try
            {
                double total = _items.Sum(i => i.Total);
                double.TryParse(txtPaidAmount.Text, out double paid);
                double remaining = total - paid;

                txtTotalAmt.Text = $"AFN {total:N0}";
                txtPaidAmt.Text = $"AFN {paid:N0}";
                txtRemainingAmt.Text = $"AFN {remaining:N0}";
            }
            catch { }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            AddItemToList();
        }

        private void AddItemToList()
        {
            try
            {
                if (_selectedMedicine == null)
                {
                    MessageBox.Show("Please select a medicine from the list!",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtMedicineSearch.Focus();
                    return;
                }

                if (!int.TryParse(txtQty.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Please enter valid quantity!",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQty.Focus();
                    return;
                }

                if (!double.TryParse(txtItemPrice.Text, out double price) || price <= 0)
                {
                    MessageBox.Show("Please enter valid price!",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtItemPrice.Focus();
                    return;
                }

                if (qty > _selectedMedicine.Stock)
                {
                    MessageBox.Show(
                        $"Not enough stock!\nAvailable: {_selectedMedicine.Stock}",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQty.Focus();
                    return;
                }

                var existing = _items.FirstOrDefault(
                    x => x.MedicineId == _selectedMedicine.Id);
                if (existing != null)
                {
                    MessageBox.Show("This medicine is already added!",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _items.Add(new SaleItem
                {
                    MedicineId = _selectedMedicine.Id,
                    MedicineName = _selectedMedicine.Name,
                    Stock = _selectedMedicine.Stock,
                    Quantity = qty,
                    Price = price,
                    Total = price * qty
                });

                // Reset for next medicine
                txtMedicineSearch.Text = "";
                txtQty.Text = "1";
                txtItemPrice.Text = "0";
                txtItemTotal.Text = "0";
                _selectedMedicine = null;
                lstMedicineSuggestions.Visibility = Visibility.Collapsed;

                dgItems.ItemsSource = null;
                dgItems.ItemsSource = _items;
                UpdateTotals();

                // Focus back to search for next medicine
                txtMedicineSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                if (btn?.Tag == null) return;
                int medicineId = Convert.ToInt32(btn.Tag);
                var item = _items.FirstOrDefault(i => i.MedicineId == medicineId);
                if (item != null)
                {
                    _items.Remove(item);
                    dgItems.ItemsSource = null;
                    dgItems.ItemsSource = _items;
                    UpdateTotals();
                }
            }
            catch { }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender,
            System.ComponentModel.CancelEventArgs e)
        {
            if (!_isSaved && _items.Count > 0)
            {
                var result = MessageBox.Show(
                    "You have unsaved items. Are you sure you want to close?",
                    "Confirm Close",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void SaveSale(bool printBill)
        {
            if (_items.Count == 0)
            {
                txtError.Text = "Please add at least one medicine!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            double.TryParse(txtPaidAmount.Text, out double paid);
            double total = _items.Sum(i => i.Total);
            double remaining = total - paid;

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    using var cmd = new SqliteCommand(@"
                        INSERT INTO Sales
                        (VoucherNo, CustomerName, TotalAmount, PaidAmount,
                         RemainingAmount, SaleDate)
                        VALUES (@vno, @customer, @total, @paid, @remaining, @date)",
                        conn, transaction);

                    cmd.Parameters.AddWithValue("@vno", txtVoucherNo.Text);
                    cmd.Parameters.AddWithValue("@customer", txtCustomer.Text.Trim());
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@paid", paid);
                    cmd.Parameters.AddWithValue("@remaining", remaining);
                    cmd.Parameters.AddWithValue("@date",
                        dpSaleDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                        DateTime.Today.ToString("yyyy-MM-dd"));
                    cmd.ExecuteNonQuery();

                    using var idCmd = new SqliteCommand(
                        "SELECT last_insert_rowid()", conn, transaction);
                    long saleId = (long)idCmd.ExecuteScalar()!;

                    foreach (var item in _items)
                    {
                        using var itemCmd = new SqliteCommand(@"
                            INSERT INTO SaleItems
                            (SaleId, MedicineId, Quantity, SalePrice, TotalPrice)
                            VALUES (@sid, @mid, @qty, @price, @total)",
                            conn, transaction);
                        itemCmd.Parameters.AddWithValue("@sid", saleId);
                        itemCmd.Parameters.AddWithValue("@mid", item.MedicineId);
                        itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@price", item.Price);
                        itemCmd.Parameters.AddWithValue("@total", item.Total);
                        itemCmd.ExecuteNonQuery();

                        using var stockCmd = new SqliteCommand(@"
                            UPDATE Medicines
                            SET Stock = Stock - @qty
                            WHERE Id = @id",
                            conn, transaction);
                        stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        stockCmd.Parameters.AddWithValue("@id", item.MedicineId);
                        stockCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    _isSaved = true;

                    LogsPage.AddLog("Insert", "Sales",
                        $"New sale - Voucher: {txtVoucherNo.Text}");

                    if (printBill)
                    {
                        var billWindow = new BillPrintWindow((int)saleId);
                        billWindow.Owner = this.Owner ?? this;
                        billWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Sale saved successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    DialogResult = true;
                    Close();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Error: {ex.Message}";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void btnSaveOnly_Click(object sender, RoutedEventArgs e)
        {
            SaveSale(false);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSale(true);
        }
    }

    public class SaleItem
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = "";
        public int Stock { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
    }

    public class MedicineStock
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double SalePrice { get; set; }
        public int Stock { get; set; }
    }
}