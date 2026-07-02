using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class LogsPage : Page
    {
        public LogsPage()
        {
            InitializeComponent();
            CreateLogsTable();
            this.Loaded += (s, e) => LoadLogs();
        }

        private void CreateLogsTable()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ActionType TEXT NOT NULL,
                        TableName TEXT,
                        Description TEXT,
                        Username TEXT DEFAULT 'admin',
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                    )", conn);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        public static void AddLog(string actionType, string tableName, string description)
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(@"
                    INSERT INTO Logs (ActionType, TableName, Description)
                    VALUES (@action, @table, @desc)", conn);
                cmd.Parameters.AddWithValue("@action", actionType);
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private void LoadLogs(string search = "", string type = "", string date = "")
        {
            try
            {
                var logs = new List<LogEntry>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = "SELECT * FROM Logs WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                    sql += " AND (Description LIKE @search OR TableName LIKE @search)";
                if (!string.IsNullOrEmpty(type) && type != "All Logs")
                    sql += " AND ActionType = @type";
                if (!string.IsNullOrEmpty(date))
                    sql += " AND date(CreatedAt) = @date";

                sql += " ORDER BY CreatedAt DESC";

                using var cmd = new SqliteCommand(sql, conn);
                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@search", $"%{search}%");
                if (!string.IsNullOrEmpty(type) && type != "All Logs")
                    cmd.Parameters.AddWithValue("@type", type);
                if (!string.IsNullOrEmpty(date))
                    cmd.Parameters.AddWithValue("@date", date);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    logs.Add(new LogEntry
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ActionType = reader["ActionType"]?.ToString() ?? "",
                        TableName = reader["TableName"]?.ToString() ?? "",
                        Description = reader["Description"]?.ToString() ?? "",
                        Username = reader["Username"]?.ToString() ?? "admin",
                        CreatedAt = reader["CreatedAt"]?.ToString() ?? ""
                    });
                }

                dgLogs.ItemsSource = logs;
                txtTotalLogs.Text = logs.Count.ToString();
                txtDeleteLogs.Text = logs.FindAll(l => l.ActionType == "Delete").Count.ToString();
                txtUpdateLogs.Text = logs.FindAll(l => l.ActionType == "Update").Count.ToString();
                txtInsertLogs.Text = logs.FindAll(l => l.ActionType == "Insert").Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (dgLogs == null) return;
                string type = (cmbLogType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                LoadLogs(txtSearch.Text, type,
                    dpFilter.SelectedDate?.ToString("yyyy-MM-dd") ?? "");
            }
            catch { }
        }

        private void cmbLogType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgLogs == null) return;
                string type = (cmbLogType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                LoadLogs(txtSearch.Text, type,
                    dpFilter.SelectedDate?.ToString("yyyy-MM-dd") ?? "");
            }
            catch { }
        }

        private void dpFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgLogs == null) return;
                string type = (cmbLogType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                LoadLogs(txtSearch.Text, type,
                    dpFilter.SelectedDate?.ToString("yyyy-MM-dd") ?? "");
            }
            catch { }
        }
    }

    public class LogEntry
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = "";
        public string TableName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Username { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }
}