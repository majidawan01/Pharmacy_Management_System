using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class EmployeesPage : Page
    {
        public EmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees(string search = "")
        {
            try
            {
                var employees = new List<Employee>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"SELECT * FROM Employees 
                               WHERE Name LIKE @search 
                               OR Phone LIKE @search
                               ORDER BY Name";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@search", $"%{search}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    employees.Add(new Employee
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"]?.ToString() ?? "",
                        Phone = reader["Phone"]?.ToString() ?? "",
                        Address = reader["Address"]?.ToString() ?? "",
                        Salary = Convert.ToDouble(reader["Salary"]),
                        JoinDate = reader["JoinDate"]?.ToString() ?? "",
                        IsActive = Convert.ToInt32(reader["IsActive"])
                    });
                }

                dgEmployees.ItemsSource = employees;
                txtTotalEmployees.Text = employees.Count.ToString();
                txtActiveEmployees.Text = employees.FindAll(
                    emp => emp.IsActive == 1).Count.ToString();

                double totalSalary = 0;
                foreach (var emp in employees)
                    totalSalary += emp.Salary;
                txtTotalSalary.Text = $"AFN {totalSalary:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadEmployees(txtSearch.Text);
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new AddEmployeeWindow();
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            int id = Convert.ToInt32(btn.Tag);
            try
            {
                var dialog = new AddEmployeeWindow(id);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                LoadEmployees();
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
                "Are you sure you want to delete this employee?",
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
                        "DELETE FROM Employees WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    LogsPage.AddLog("Delete", "Employees", $"Employee deleted - ID: {id}");
                    LoadEmployees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public double Salary { get; set; }
        public string JoinDate { get; set; } = "";
        public int IsActive { get; set; }
    }
}