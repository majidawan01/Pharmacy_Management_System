using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace Pharmacy_Management_System.Views
{
    public partial class AddEmployeeWindow : Window
    {
        private int _employeeId = 0;

        public AddEmployeeWindow(int employeeId = 0)
        {
            InitializeComponent();
            _employeeId = employeeId;
            dpJoinDate.SelectedDate = DateTime.Today;

            if (_employeeId > 0)
            {
                txtTitle.Text = "Edit Employee";
                btnSave.Content = "Update Employee";
                LoadEmployeeData();
            }
        }

        private void LoadEmployeeData()
        {
            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT * FROM Employees WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _employeeId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtName.Text = reader["Name"]?.ToString() ?? "";
                    txtPhone.Text = reader["Phone"]?.ToString() ?? "";
                    txtAddress.Text = reader["Address"]?.ToString() ?? "";
                    txtSalary.Text = reader["Salary"]?.ToString() ?? "0";
                    cmbStatus.SelectedIndex =
                        Convert.ToInt32(reader["IsActive"]) == 1 ? 0 : 1;
                    if (DateTime.TryParse(reader["JoinDate"]?.ToString(),
                        out DateTime d))
                        dpJoinDate.SelectedDate = d;
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
                txtError.Text = "Employee name is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (!double.TryParse(txtSalary.Text, out double salary))
            {
                txtError.Text = "Invalid salary amount!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            int isActive = cmbStatus.SelectedIndex == 0 ? 1 : 0;

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                string sql;
                if (_employeeId == 0)
                {
                    sql = @"INSERT INTO Employees 
                            (Name, Phone, Address, Salary, JoinDate, IsActive)
                            VALUES (@name, @phone, @address, @salary, @joinDate, @isActive)";
                }
                else
                {
                    sql = @"UPDATE Employees SET
                            Name=@name, Phone=@phone, Address=@address,
                            Salary=@salary, JoinDate=@joinDate, IsActive=@isActive
                            WHERE Id=@id";
                }

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@salary", salary);
                cmd.Parameters.AddWithValue("@joinDate",
                    dpJoinDate.SelectedDate?.ToString("yyyy-MM-dd") ??
                    DateTime.Today.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@isActive", isActive);
                if (_employeeId > 0)
                    cmd.Parameters.AddWithValue("@id", _employeeId);

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