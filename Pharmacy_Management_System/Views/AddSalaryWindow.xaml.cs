using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy_Management_System.Views
{
    public partial class AddSalaryWindow : Window
    {
        private List<Employee> _employees = new();

        public AddSalaryWindow()
        {
            InitializeComponent();
            dpPaidDate.SelectedDate = DateTime.Today;
            txtMonth.Text = DateTime.Now.ToString("yyyy-MM");
            this.Loaded += (s, e) => LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                _employees = new List<Employee>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT Id, Name, Salary FROM Employees WHERE IsActive=1 ORDER BY Name",
                    conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _employees.Add(new Employee
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Salary = reader.GetDouble(2)
                    });
                }
                cmbEmployee.ItemsSource = null;
                cmbEmployee.ItemsSource = _employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void cmbEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is Employee emp)
                txtAmount.Text = emp.Salary.ToString("N2");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmployee.SelectedItem is not Employee emp)
            {
                txtError.Text = "Please select an employee!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (!double.TryParse(txtAmount.Text, out double amount) || amount <= 0)
            {
                txtError.Text = "Please enter valid amount!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql = @"INSERT INTO Salaries 
                    (EmployeeId, Amount, Month, PaidDate, Notes)
                    VALUES (@empId, @amount, @month, @date, @notes)";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@empId", emp.Id);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@month", txtMonth.Text.Trim());
                cmd.Parameters.AddWithValue("@date",
                    dpPaidDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                    DateTime.Today.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@notes", txtNotes.Text.Trim());
                cmd.ExecuteNonQuery();

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