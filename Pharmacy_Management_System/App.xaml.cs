using Microsoft.Data.Sqlite;
using Pharmacy_Management_System.Database;
using Pharmacy_Management_System.Views;
using System.Windows;

namespace Pharmacy_Management_System
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                var sqlitePcl = System.Type.GetType("SQLitePCL.Batteries, SQLitePCLRaw.core")
                               ?? System.Type.GetType("SQLitePCL.Batteries, SQLitePCLRaw.bundle_e_sqlite3");
                if (sqlitePcl != null)
                {
                    var init = sqlitePcl.GetMethod("Init",
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Static);
                    init?.Invoke(null, null);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQLitePCL init error: {ex.Message}");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("Starting database initialization...");
                DatabaseHelper.InitializeDatabase();
                //ResetAdminPassword();
                System.Diagnostics.Debug.WriteLine("Database initialized successfully.");
            }
            catch (System.Exception dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Database error: {dbEx.Message}");
                MessageBox.Show($"Database initialization failed: {dbEx.Message}\n\n{dbEx.StackTrace}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("Creating LoginWindow...");
                var loginWindow = new LoginWindow();
                System.Diagnostics.Debug.WriteLine("Showing LoginWindow...");
                loginWindow.Show();
                System.Diagnostics.Debug.WriteLine("LoginWindow displayed.");
            }
            catch (System.Exception winEx)
            {
                System.Diagnostics.Debug.WriteLine($"Window error: {winEx.Message}");
                MessageBox.Show($"Failed to show login window: {winEx.Message}\n\n{winEx.StackTrace}",
                    "Window Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown(1);
            }
        }

        //private void ResetAdminPassword()
        //{
        //    try
        //    {
        //        using var conn = DatabaseHelper.GetConnection();
        //        conn.Open();
        //        using var cmd = new SqliteCommand(
        //            @"INSERT OR REPLACE INTO Users (Username, Password, Role) 
        //              VALUES ('admin', 'admin123', 'Admin')", conn);
        //        cmd.ExecuteNonQuery();
        //        System.Diagnostics.Debug.WriteLine("Admin password reset successfully.");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"ResetAdminPassword error: {ex.Message}");
        //    }
        }
    }
