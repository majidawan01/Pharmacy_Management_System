using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pharmacy_Management_System.Views
{
    public partial class AddPurchaseWindow : Window
    {
        private List<PurchaseItem> _items = new();
        private List<PurchaseMedicine> _medicines = new();
        private PurchaseMedicine? _selectedMedicine = null;
        private bool _isSaved = false;
        private int _purchaseId = 0;
        private bool _isViewMode = false;

        public AddPurchaseWindow(int purchaseId = 0, bool viewMode = false)
        {
            InitializeComponent();
            _purchaseId = purchaseId;
            _isViewMode = viewMode;

            txtVoucherNo.Text = $"PUR-{DateTime.Now:yyyyMMdd-HHmmss}";
            dpPurchaseDate.SelectedDate = DateTime.Today;

            this.Loaded += (s, e) =>
            {
                LoadMedicines();
                if (_purchaseId > 0)
                    LoadPurchaseData();
                else
                    txtMedicineSearch.Focus();
            };
        }

        private void LoadMedicines()
        {
            try
            {
                _medicines = new List<PurchaseMedicine>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT Id, Name, PurchasePrice FROM Medicines ORDER BY Name",
                    conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _medicines.Add(new PurchaseMedicine
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        PurchasePrice = reader.GetDouble(2)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading medicines: {ex.Message}");
            }
        }

        private void LoadPurchaseData()
        {
            try
            {
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
                    txtPaidAmount.Text = reader["PaidAmount"]?.ToString() ?? "0";

                    if (DateTime.TryParse(
                        reader["PurchaseDate"]?.ToString(), out DateTime date))
                        dpPurchaseDate.SelectedDate = date;
                }
                reader.Close();

                using var itemCmd = new SqliteCommand(@"
                    SELECT pi.MedicineId, m.Name, pi.Quantity,
                           pi.PurchasePrice, pi.TotalPrice
                    FROM PurchaseItems pi
                    JOIN Medicines m ON pi.MedicineId = m.Id
                    WHERE pi.PurchaseId = @id", conn);
                itemCmd.Parameters.AddWithValue("@id", _purchaseId);
                using var itemReader = itemCmd.ExecuteReader();

                while (itemReader.Read())
                {
                    _items.Add(new PurchaseItem
                    {
                        MedicineId = Convert.ToInt32(itemReader["MedicineId"]),
                        MedicineName = itemReader["Name"]?.ToString() ?? "",
                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                        Price = Convert.ToDouble(itemReader["PurchasePrice"]),
                        Total = Convert.ToDouble(itemReader["TotalPrice"])
                    });
                }

                dgItems.ItemsSource = null;
                dgItems.ItemsSource = _items;
                UpdateTotals();

                if (_isViewMode)
                {
                    txtWindowTitle.Text = "View Purchase";
                    SetViewMode();
                }
                else
                {
                    txtWindowTitle.Text = "Edit Purchase";
                    SetEditMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase: {ex.Message}");
            }
        }

        private void SetViewMode()
        {
            txtSupplier.IsReadOnly = true;
            txtPaidAmount.IsReadOnly = true;
            dpPurchaseDate.IsEnabled = false;
            txtMedicineSearch.IsReadOnly = true;
            txtQty.IsReadOnly = true;
            txtItemPrice.IsReadOnly = true;
            btnAddItem.IsEnabled = false;
            btnAddItem.Opacity = 0.4;
            btnSave.Visibility = Visibility.Collapsed;
            btnEdit.Visibility = Visibility.Visible;
        }

        private void SetEditMode()
        {
            txtWindowTitle.Text = "Edit Purchase";
            txtSupplier.IsReadOnly = false;
            txtPaidAmount.IsReadOnly = false;
            dpPurchaseDate.IsEnabled = true;
            txtMedicineSearch.IsReadOnly = false;
            txtQty.IsReadOnly = false;
            txtItemPrice.IsReadOnly = false;
            btnAddItem.IsEnabled = true;
            btnAddItem.Opacity = 1;
            btnSave.Content = "💾 Update Purchase";
            btnSave.Visibility = Visibility.Visible;
            btnEdit.Visibility = Visibility.Collapsed;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode();
        }

        private void txtMedicineSearch_TextChanged(object sender,
            TextChangedEventArgs e)
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
                        .Select(m => m.Name).ToList();
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
                    SelectMedicine();
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

                string name = lstMedicineSuggestions.SelectedItem.ToString() ?? "";
                _selectedMedicine = _medicines.FirstOrDefault(m => m.Name == name);

                if (_selectedMedicine != null &&
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

                string name = lstMedicineSuggestions.SelectedItem.ToString() ?? "";
                _selectedMedicine = _medicines.FirstOrDefault(m => m.Name == name);

                if (_selectedMedicine != null)
                {
                    txtMedicineSearch.Text = _selectedMedicine.Name;
                    txtItemPrice.Text = _selectedMedicine.PurchasePrice.ToString();
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
                    AddItemToList();
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

                var existing = _items.FirstOrDefault(
                    x => x.MedicineId == _selectedMedicine.Id);
                if (existing != null)
                {
                    MessageBox.Show("This medicine is already added!",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _items.Add(new PurchaseItem
                {
                    MedicineId = _selectedMedicine.Id,
                    MedicineName = _selectedMedicine.Name,
                    Quantity = qty,
                    Price = price,
                    Total = price * qty
                });

                txtMedicineSearch.Text = "";
                txtQty.Text = "1";
                txtItemPrice.Text = "0";
                txtItemTotal.Text = "0";
                _selectedMedicine = null;
                lstMedicineSuggestions.Visibility = Visibility.Collapsed;

                dgItems.ItemsSource = null;
                dgItems.ItemsSource = _items;
                UpdateTotals();

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
            if (!_isSaved && _items.Count > 0 && _purchaseId == 0)
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_purchaseId > 0)
                UpdatePurchase();
            else
                SavePurchase();
        }

        private void SavePurchase()
        {
            if (string.IsNullOrWhiteSpace(txtSupplier.Text))
            {
                txtError.Text = "Supplier name is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

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
                        INSERT INTO Purchases
                        (VoucherNo, SupplierName, TotalAmount, PaidAmount,
                         RemainingAmount, PurchaseDate)
                        VALUES (@vno, @supplier, @total, @paid, @remaining, @date)",
                        conn, transaction);

                    cmd.Parameters.AddWithValue("@vno", txtVoucherNo.Text);
                    cmd.Parameters.AddWithValue("@supplier", txtSupplier.Text.Trim());
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@paid", paid);
                    cmd.Parameters.AddWithValue("@remaining", remaining);
                    cmd.Parameters.AddWithValue("@date",
                        dpPurchaseDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                        DateTime.Today.ToString("yyyy-MM-dd"));
                    cmd.ExecuteNonQuery();

                    using var idCmd = new SqliteCommand(
                        "SELECT last_insert_rowid()", conn, transaction);
                    long purchaseId = (long)idCmd.ExecuteScalar()!;

                    foreach (var item in _items)
                    {
                        using var itemCmd = new SqliteCommand(@"
                            INSERT INTO PurchaseItems
                            (PurchaseId, MedicineId, Quantity, PurchasePrice, TotalPrice)
                            VALUES (@pid, @mid, @qty, @price, @total)",
                            conn, transaction);
                        itemCmd.Parameters.AddWithValue("@pid", purchaseId);
                        itemCmd.Parameters.AddWithValue("@mid", item.MedicineId);
                        itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@price", item.Price);
                        itemCmd.Parameters.AddWithValue("@total", item.Total);
                        itemCmd.ExecuteNonQuery();

                        using var stockCmd = new SqliteCommand(@"
                            UPDATE Medicines
                            SET Stock = Stock + @qty
                            WHERE Id = @id",
                            conn, transaction);
                        stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        stockCmd.Parameters.AddWithValue("@id", item.MedicineId);
                        stockCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    _isSaved = true;

                    LogsPage.AddLog("Insert", "Purchases",
                        $"New purchase - Voucher: {txtVoucherNo.Text}");

                    var printResult = MessageBox.Show(
                        "Purchase saved! Do you want to print voucher?",
                        "Print Voucher",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (printResult == MessageBoxResult.Yes)
                    {
                        var voucherWindow = new PurchaseVoucherWindow((int)purchaseId);
                        voucherWindow.Owner = this.Owner ?? this;
                        voucherWindow.ShowDialog();
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

        private void UpdatePurchase()
        {
            if (string.IsNullOrWhiteSpace(txtSupplier.Text))
            {
                txtError.Text = "Supplier name is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

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
                        UPDATE Purchases SET
                        SupplierName=@supplier,
                        TotalAmount=@total,
                        PaidAmount=@paid,
                        RemainingAmount=@remaining,
                        PurchaseDate=@date
                        WHERE Id=@id",
                        conn, transaction);

                    cmd.Parameters.AddWithValue("@supplier", txtSupplier.Text.Trim());
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@paid", paid);
                    cmd.Parameters.AddWithValue("@remaining", remaining);
                    cmd.Parameters.AddWithValue("@date",
                        dpPurchaseDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                        DateTime.Today.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@id", _purchaseId);
                    cmd.ExecuteNonQuery();

                    using var deleteCmd = new SqliteCommand(
                        "DELETE FROM PurchaseItems WHERE PurchaseId=@id",
                        conn, transaction);
                    deleteCmd.Parameters.AddWithValue("@id", _purchaseId);
                    deleteCmd.ExecuteNonQuery();

                    foreach (var item in _items)
                    {
                        using var itemCmd = new SqliteCommand(@"
                            INSERT INTO PurchaseItems
                            (PurchaseId, MedicineId, Quantity, PurchasePrice, TotalPrice)
                            VALUES (@pid, @mid, @qty, @price, @total)",
                            conn, transaction);
                        itemCmd.Parameters.AddWithValue("@pid", _purchaseId);
                        itemCmd.Parameters.AddWithValue("@mid", item.MedicineId);
                        itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@price", item.Price);
                        itemCmd.Parameters.AddWithValue("@total", item.Total);
                        itemCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    _isSaved = true;

                    LogsPage.AddLog("Update", "Purchases",
                        $"Purchase updated - ID: {_purchaseId}");

                    MessageBox.Show("Purchase updated successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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
    }

    public class PurchaseItem
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = "";
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Total { get; set; }
    }

    public class PurchaseMedicine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double PurchasePrice { get; set; }
    }
}