using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Pharmacy_Management_System.Database
{
    public class DatabaseHelper
    {
        private static string? dbPath;

        private static string GetDbPath()
        {
            if (dbPath != null) return dbPath;

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(appDataPath))
                appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            dbPath = Path.Combine(appDataPath, "PharmacyMS", "pharmacy.db");
            return dbPath;
        }

        public static SqliteConnection GetConnection()
        {
            var path = GetDbPath();
            string? folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return new SqliteConnection($"Data Source={path}");
        }

        public static void InitializeDatabase()
        {
            using var conn = GetConnection();
            conn.Open();
            CreateTables(conn);
        }

        private static void CreateTables(SqliteConnection conn)
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Role TEXT NOT NULL,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Medicines (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT UNIQUE,
                Name TEXT NOT NULL,
                GenericName TEXT,
                Company TEXT,
                Category TEXT,
                Unit TEXT,
                PurchasePrice REAL DEFAULT 0,
                SalePrice REAL DEFAULT 0,
                Stock INTEGER DEFAULT 0,
                MinStock INTEGER DEFAULT 10,
                ExpiryDate TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Purchases (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                VoucherNo TEXT UNIQUE,
                SupplierName TEXT,
                TotalAmount REAL DEFAULT 0,
                PaidAmount REAL DEFAULT 0,
                RemainingAmount REAL DEFAULT 0,
                PurchaseDate TEXT DEFAULT CURRENT_TIMESTAMP,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS PurchaseItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PurchaseId INTEGER,
                MedicineId INTEGER,
                Quantity INTEGER,
                PurchasePrice REAL,
                TotalPrice REAL,
                ExpiryDate TEXT,
                FOREIGN KEY(PurchaseId) REFERENCES Purchases(Id),
                FOREIGN KEY(MedicineId) REFERENCES Medicines(Id)
            );

            CREATE TABLE IF NOT EXISTS Sales (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                VoucherNo TEXT UNIQUE,
                CustomerName TEXT,
                TotalAmount REAL DEFAULT 0,
                PaidAmount REAL DEFAULT 0,
                RemainingAmount REAL DEFAULT 0,
                SaleDate TEXT DEFAULT CURRENT_TIMESTAMP,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS SaleItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SaleId INTEGER,
                MedicineId INTEGER,
                Quantity INTEGER,
                SalePrice REAL,
                TotalPrice REAL,
                FOREIGN KEY(SaleId) REFERENCES Sales(Id),
                FOREIGN KEY(MedicineId) REFERENCES Medicines(Id)
            );

            CREATE TABLE IF NOT EXISTS Expenses (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Amount REAL DEFAULT 0,
                Category TEXT,
                ExpenseDate TEXT DEFAULT CURRENT_TIMESTAMP,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Phone TEXT,
                Address TEXT,
                Salary REAL DEFAULT 0,
                JoinDate TEXT,
                IsActive INTEGER DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Salaries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeId INTEGER,
                Amount REAL DEFAULT 0,
                Month TEXT,
                PaidDate TEXT DEFAULT CURRENT_TIMESTAMP,
                Notes TEXT,
                FOREIGN KEY(EmployeeId) REFERENCES Employees(Id)
            );

            CREATE TABLE IF NOT EXISTS Debts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT NOT NULL,
                Phone TEXT,
                TotalAmount REAL DEFAULT 0,
                PaidAmount REAL DEFAULT 0,
                RemainingAmount REAL DEFAULT 0,
                DebtDate TEXT DEFAULT CURRENT_TIMESTAMP,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS Returns (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Type TEXT NOT NULL,
                VoucherNo TEXT,
                PartyName TEXT,
                TotalAmount REAL DEFAULT 0,
                ReturnDate TEXT DEFAULT CURRENT_TIMESTAMP,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS Suppliers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Phone TEXT,
                Address TEXT,
                Balance REAL DEFAULT 0,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Vouchers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                VoucherNo TEXT UNIQUE,
                VoucherType TEXT NOT NULL,
                PartyName TEXT,
                Amount REAL DEFAULT 0,
                Description TEXT,
                VoucherDate TEXT DEFAULT CURRENT_TIMESTAMP,
                CreatedBy TEXT
            );

            CREATE TABLE IF NOT EXISTS SyncLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TableName TEXT NOT NULL,
                RecordId INTEGER,
                Action TEXT NOT NULL,
                SyncStatus TEXT DEFAULT 'Pending',
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            string adminSql = @"
            INSERT OR IGNORE INTO Users (Username, Password, Role) 
            VALUES ('admin', 'admin123', 'Admin');";
            using var adminCmd = new SqliteCommand(adminSql, conn);
            adminCmd.ExecuteNonQuery();
        }
    }
}