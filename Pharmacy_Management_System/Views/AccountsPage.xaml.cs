using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class AccountsPage : Page
    {
        public AccountsPage()
        {
            InitializeComponent();
            CreateAccountsTable();
            LoadAccounts();
        }

        private void CreateAccountsTable()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS Accounts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CompanyName TEXT NOT NULL,
                        AccountType TEXT NOT NULL,
                        TotalAmount REAL DEFAULT 0,
                        PaidAmount REAL DEFAULT 0,
                        RemainingAmount REAL DEFAULT 0,
                        AccountDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        Notes TEXT
                    )", conn);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private void LoadAccounts(string search = "")
        {
            try
            {
                var accounts = new List<Account>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Accounts 
                               WHERE CompanyName LIKE @search
                               ORDER BY AccountDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    accounts.Add(new Account
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CompanyName = reader["CompanyName"]?.ToString() ?? "",
                        AccountType = reader["AccountType"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDouble(reader["TotalAmount"]),
                        PaidAmount = Convert.ToDouble(reader["PaidAmount"]),
                        RemainingAmount = Convert.ToDouble(reader["RemainingAmount"]),
                        AccountDate = reader["AccountDate"]?.ToString() ?? "",
                        Notes = reader["Notes"]?.ToString() ?? ""
                    });
                }

                dgAccounts.ItemsSource = accounts;
                txtTotalAccounts.Text = accounts.Count.ToString();

                double receivable = 0, payable = 0;
                foreach (var acc in accounts)
                {
                    if (acc.AccountType == "Receivable")
                        receivable += acc.RemainingAmount;
                    else
                        payable += acc.RemainingAmount;
                }

                txtTotalReceivable.Text = $"AFN {receivable:N0}";
                txtTotalPayable.Text = $"AFN {payable:N0}";
                txtNetBalance.Text = $"AFN {(receivable - payable):N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAccounts(txtSearch.Text);
        }

        private void btnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddAccountWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            int id = Convert.ToInt32(btn.Tag);
            try
            {
                var dialog = new AddAccountWindow(id, isPayment: true);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadAccounts();
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
                "Are you sure you want to delete this account?",
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
                        "DELETE FROM Accounts WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    LoadAccounts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class Account
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string AccountType { get; set; } = "";
        public double TotalAmount { get; set; }
        public double PaidAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string AccountDate { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}