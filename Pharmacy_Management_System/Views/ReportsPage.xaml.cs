using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Pharmacy_Management_System.Views
{
    public partial class ReportsPage : Page
    {
        private string _currentReport = "";

        private const string CompanyName = "SHAKIR IHSAN";
        private const string CompanySubtitle = "WHOLE SELLER";
        private const string CompanyAddress = "Laghman Hada, Gulab Sehat Plaza";
        private const string CompanyPhone = "+93770212223 | +93777740095";
        private const string CompanyEmail = "usman.fazli1000@gmail.com";
        private const string CompanyDeveloper = "Afghan Cosmos IT & Solutions";

        public ReportsPage()
        {
            InitializeComponent();
            dpFrom.SelectedDate = DateTime.Today;
            dpTo.SelectedDate = DateTime.Today;
        }

        #region Navigation & Filter

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;
            _currentReport = btn.Tag.ToString() ?? "";
            LoadReport();
        }

        private void DateFilter_Changed(object sender,
            SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentReport))
                LoadReport();
        }

        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = DateTime.Today;
            dpTo.SelectedDate = DateTime.Today;
        }

        private void btnThisWeek_Click(object sender, RoutedEventArgs e)
        {
            int diff = (7 + (int)DateTime.Today.DayOfWeek -
                (int)DayOfWeek.Monday) % 7;
            dpFrom.SelectedDate = DateTime.Today.AddDays(-diff);
            dpTo.SelectedDate = DateTime.Today;
        }

        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = new DateTime(
                DateTime.Today.Year, DateTime.Today.Month, 1);
            dpTo.SelectedDate = DateTime.Today;
        }

        private void btnThisYear_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = new DateTime(
                DateTime.Today.Year, 1, 1);
            dpTo.SelectedDate = DateTime.Today;
        }

        #endregion

        #region Load Reports

        private void LoadReport()
        {
            try
            {
                string from = dpFrom.SelectedDate?
                    .ToString("yyyy-MM-dd")
                    ?? DateTime.Today.ToString("yyyy-MM-dd");
                string to = dpTo.SelectedDate?
                    .ToString("yyyy-MM-dd")
                    ?? DateTime.Today.ToString("yyyy-MM-dd");

                switch (_currentReport)
                {
                    case "SalesDaily":
                    case "SalesWeekly":
                    case "SalesMonthly":
                    case "SalesYearly":
                    case "SalesCustom":
                        LoadSalesReport(from, to); break;
                    case "PurchaseDaily":
                    case "PurchaseMonthly":
                    case "PurchaseYearly":
                        LoadPurchaseReport(from, to); break;
                    case "StockAll":
                        LoadStockReport(); break;
                    case "StockLow":
                        LoadLowStockReport(); break;
                    case "StockExpiry":
                        LoadExpiryReport(); break;
                    case "ExpenseDaily":
                    case "ExpenseMonthly":
                    case "ExpenseYearly":
                        LoadExpenseReport(from, to); break;
                    case "SalaryReport":
                        LoadSalaryReport(from, to); break;
                    case "DebtReport":
                        LoadDebtReport(); break;
                    case "MasterReport":
                        LoadMasterReport(from, to); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void LoadSalesReport(string from, string to)
        {
            txtReportTitle.Text = "Sales Report";
            txtReportSubTitle.Text = $"From {from} To {to}";

            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT VoucherNo, CustomerName,
                           TotalAmount, PaidAmount,
                           RemainingAmount, SaleDate
                           FROM Sales
                           WHERE date(SaleDate) BETWEEN @from AND @to
                           ORDER BY SaleDate DESC";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            double total = 0, paid = 0, remaining = 0;
            int count = 0;
            var list = new List<SalesReportItem>();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                double t = Convert.ToDouble(reader["TotalAmount"]);
                double p = Convert.ToDouble(reader["PaidAmount"]);
                double r = Convert.ToDouble(reader["RemainingAmount"]);
                total += t; paid += p; remaining += r; count++;
                list.Add(new SalesReportItem
                {
                    VoucherNo = reader["VoucherNo"]?.ToString() ?? "",
                    CustomerName =
                        reader["CustomerName"]?.ToString() ?? "",
                    TotalAmount = t,
                    PaidAmount = p,
                    RemainingAmount = r,
                    SaleDate = reader["SaleDate"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Total Sales";
            txtCard1Value.Text = count.ToString();
            txtCard2Label.Text = "Total Amount";
            txtCard2Value.Text = $"AFN {total:N0}";
            txtCard3Label.Text = "Paid Amount";
            txtCard3Value.Text = $"AFN {paid:N0}";
            txtCard4Label.Text = "Remaining";
            txtCard4Value.Text = $"AFN {remaining:N0}";
        }

        private void LoadPurchaseReport(string from, string to)
        {
            txtReportTitle.Text = "Purchase Report";
            txtReportSubTitle.Text = $"From {from} To {to}";

            var list = new List<PurchaseReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT VoucherNo, SupplierName,
                           TotalAmount, PaidAmount,
                           RemainingAmount, PurchaseDate
                           FROM Purchases
                           WHERE date(PurchaseDate) BETWEEN @from AND @to
                           ORDER BY PurchaseDate DESC";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            double total = 0, paid = 0, remaining = 0;
            int count = 0;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                double t = Convert.ToDouble(reader["TotalAmount"]);
                double p = Convert.ToDouble(reader["PaidAmount"]);
                double r = Convert.ToDouble(reader["RemainingAmount"]);
                total += t; paid += p; remaining += r; count++;
                list.Add(new PurchaseReportItem
                {
                    VoucherNo = reader["VoucherNo"]?.ToString() ?? "",
                    SupplierName =
                        reader["SupplierName"]?.ToString() ?? "",
                    TotalAmount = t,
                    PaidAmount = p,
                    RemainingAmount = r,
                    PurchaseDate =
                        reader["PurchaseDate"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Total Purchases";
            txtCard1Value.Text = count.ToString();
            txtCard2Label.Text = "Total Amount";
            txtCard2Value.Text = $"AFN {total:N0}";
            txtCard3Label.Text = "Paid Amount";
            txtCard3Value.Text = $"AFN {paid:N0}";
            txtCard4Label.Text = "Remaining";
            txtCard4Value.Text = $"AFN {remaining:N0}";
        }

        private void LoadStockReport()
        {
            txtReportTitle.Text = "All Stock Report";
            txtReportSubTitle.Text = "Complete medicine inventory";

            var list = new List<StockReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                @"SELECT Code, Name, GenericName, Company,
                  PurchasePrice, SalePrice, Stock,
                  MinStock, ExpiryDate
                  FROM Medicines ORDER BY Name", conn);

            double totalValue = 0;
            int count = 0, lowCount = 0;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int stock = Convert.ToInt32(reader["Stock"]);
                double pp = Convert.ToDouble(reader["PurchasePrice"]);
                int minStock = Convert.ToInt32(reader["MinStock"]);
                count++;
                if (stock <= minStock) lowCount++;
                totalValue += stock * pp;
                list.Add(new StockReportItem
                {
                    Code = reader["Code"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    GenericName =
                        reader["GenericName"]?.ToString() ?? "",
                    Company = reader["Company"]?.ToString() ?? "",
                    PurchasePrice = pp,
                    SalePrice =
                        Convert.ToDouble(reader["SalePrice"]),
                    Stock = stock,
                    ExpiryDate =
                        reader["ExpiryDate"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Total Medicines";
            txtCard1Value.Text = count.ToString();
            txtCard2Label.Text = "Stock Value";
            txtCard2Value.Text = $"AFN {totalValue:N0}";
            txtCard3Label.Text = "Low Stock Items";
            txtCard3Value.Text = lowCount.ToString();
            txtCard4Label.Text = "Total Items";
            txtCard4Value.Text = count.ToString();
        }

        private void LoadLowStockReport()
        {
            txtReportTitle.Text = "Low Stock Report";
            txtReportSubTitle.Text = "Medicines that need restocking";

            var list = new List<StockReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                @"SELECT Code, Name, GenericName, Company,
                  PurchasePrice, SalePrice, Stock,
                  MinStock, ExpiryDate
                  FROM Medicines
                  WHERE Stock <= MinStock ORDER BY Stock", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new StockReportItem
                {
                    Code = reader["Code"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    GenericName =
                        reader["GenericName"]?.ToString() ?? "",
                    Company = reader["Company"]?.ToString() ?? "",
                    PurchasePrice =
                        Convert.ToDouble(reader["PurchasePrice"]),
                    SalePrice =
                        Convert.ToDouble(reader["SalePrice"]),
                    Stock = Convert.ToInt32(reader["Stock"]),
                    ExpiryDate =
                        reader["ExpiryDate"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Low Stock Items";
            txtCard1Value.Text = list.Count.ToString();
            txtCard2Label.Text = "Status";
            txtCard2Value.Text = "Need Restock";
            txtCard3Label.Text = "";
            txtCard3Value.Text = "";
            txtCard4Label.Text = "";
            txtCard4Value.Text = "";
        }

        private void LoadExpiryReport()
        {
            txtReportTitle.Text = "Near Expiry Report";
            txtReportSubTitle.Text =
                "Medicines expiring within 30 days";

            var list = new List<StockReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            string expiryDate =
                DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");
            using var cmd = new SqliteCommand(
                @"SELECT Code, Name, GenericName, Company,
                  PurchasePrice, SalePrice, Stock,
                  MinStock, ExpiryDate
                  FROM Medicines
                  WHERE ExpiryDate != ''
                  AND date(ExpiryDate) <= @expiry
                  ORDER BY ExpiryDate", conn);
            cmd.Parameters.AddWithValue("@expiry", expiryDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new StockReportItem
                {
                    Code = reader["Code"]?.ToString() ?? "",
                    Name = reader["Name"]?.ToString() ?? "",
                    GenericName =
                        reader["GenericName"]?.ToString() ?? "",
                    Company = reader["Company"]?.ToString() ?? "",
                    PurchasePrice =
                        Convert.ToDouble(reader["PurchasePrice"]),
                    SalePrice =
                        Convert.ToDouble(reader["SalePrice"]),
                    Stock = Convert.ToInt32(reader["Stock"]),
                    ExpiryDate =
                        reader["ExpiryDate"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Expiring Soon";
            txtCard1Value.Text = list.Count.ToString();
            txtCard2Label.Text = "Status";
            txtCard2Value.Text = "Check Stock";
            txtCard3Label.Text = "";
            txtCard3Value.Text = "";
            txtCard4Label.Text = "";
            txtCard4Value.Text = "";
        }

        private void LoadExpenseReport(string from, string to)
        {
            txtReportTitle.Text = "Expense Report";
            txtReportSubTitle.Text = $"From {from} To {to}";

            var list = new List<ExpenseReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT Title, Category, Amount,
                           ExpenseDate, Notes
                           FROM Expenses
                           WHERE date(ExpenseDate) BETWEEN @from AND @to
                           ORDER BY ExpenseDate DESC";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            double total = 0;
            int count = 0;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                double amt = Convert.ToDouble(reader["Amount"]);
                total += amt; count++;
                list.Add(new ExpenseReportItem
                {
                    Title = reader["Title"]?.ToString() ?? "",
                    Category = reader["Category"]?.ToString() ?? "",
                    Amount = amt,
                    ExpenseDate =
                        reader["ExpenseDate"]?.ToString() ?? "",
                    Notes = reader["Notes"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Total Expenses";
            txtCard1Value.Text = count.ToString();
            txtCard2Label.Text = "Total Amount";
            txtCard2Value.Text = $"AFN {total:N0}";
            txtCard3Label.Text = "";
            txtCard3Value.Text = "";
            txtCard4Label.Text = "";
            txtCard4Value.Text = "";
        }

        private void LoadSalaryReport(string from, string to)
        {
            txtReportTitle.Text = "Salary Report";
            txtReportSubTitle.Text = $"From {from} To {to}";

            var list = new List<SalaryReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT e.Name, s.Amount,
                           s.Month, s.PaidDate, s.Notes
                           FROM Salaries s
                           JOIN Employees e ON s.EmployeeId = e.Id
                           WHERE date(s.PaidDate) BETWEEN @from AND @to
                           ORDER BY s.PaidDate DESC";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            double total = 0;
            int count = 0;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                double amt = Convert.ToDouble(reader["Amount"]);
                total += amt; count++;
                list.Add(new SalaryReportItem
                {
                    EmployeeName = reader["Name"]?.ToString() ?? "",
                    Amount = amt,
                    Month = reader["Month"]?.ToString() ?? "",
                    PaidDate = reader["PaidDate"]?.ToString() ?? "",
                    Notes = reader["Notes"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Total Records";
            txtCard1Value.Text = count.ToString();
            txtCard2Label.Text = "Total Paid";
            txtCard2Value.Text = $"AFN {total:N0}";
            txtCard3Label.Text = "";
            txtCard3Value.Text = "";
            txtCard4Label.Text = "";
            txtCard4Value.Text = "";
        }

        private void LoadDebtReport()
        {
            txtReportTitle.Text = "Debt Report";
            txtReportSubTitle.Text = "All customer debts";

            var list = new List<DebtReportItem>();
            using var conn = Database.DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                @"SELECT CustomerName, Phone, TotalAmount,
                  PaidAmount, RemainingAmount, DebtDate
                  FROM Debts ORDER BY DebtDate DESC", conn);

            double total = 0, remaining = 0;
            int count = 0;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                double t = Convert.ToDouble(reader["TotalAmount"]);
                double r = Convert.ToDouble(reader["RemainingAmount"]);
                total += t; remaining += r; count++;
                list.Add(new DebtReportItem
                {
                    CustomerName =
                        reader["CustomerName"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    TotalAmount = t,
                    PaidAmount =
                        Convert.ToDouble(reader["PaidAmount"]),
                    RemainingAmount = r,
                    DebtDate = reader["DebtDate"]?.ToString() ?? ""
                });
            }

            dgReport.ItemsSource = list;
            txtCard1Label.Text = "Total Debts";
            txtCard1Value.Text = count.ToString();
            txtCard2Label.Text = "Total Amount";
            txtCard2Value.Text = $"AFN {total:N0}";
            txtCard3Label.Text = "Remaining";
            txtCard3Value.Text = $"AFN {remaining:N0}";
            txtCard4Label.Text = "";
            txtCard4Value.Text = "";
        }

        private void LoadMasterReport(string from, string to)
        {
            txtReportTitle.Text = "Master Report";
            txtReportSubTitle.Text =
                $"Complete Summary from {from} To {to}";

            try
            {
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();

                using var salesCmd = new SqliteCommand(
                    @"SELECT COALESCE(SUM(TotalAmount),0),
                      COALESCE(SUM(PaidAmount),0),
                      COALESCE(SUM(RemainingAmount),0), COUNT(*)
                      FROM Sales
                      WHERE date(SaleDate) BETWEEN @from AND @to",
                    conn);
                salesCmd.Parameters.AddWithValue("@from", from);
                salesCmd.Parameters.AddWithValue("@to", to);
                using var sr = salesCmd.ExecuteReader();
                double salesTotal = 0, salesPaid = 0,
                    salesRemaining = 0;
                int salesCount = 0;
                if (sr.Read())
                {
                    salesTotal = Convert.ToDouble(sr[0]);
                    salesPaid = Convert.ToDouble(sr[1]);
                    salesRemaining = Convert.ToDouble(sr[2]);
                    salesCount = Convert.ToInt32(sr[3]);
                }
                sr.Close();

                using var purCmd = new SqliteCommand(
                    @"SELECT COALESCE(SUM(TotalAmount),0),
                      COALESCE(SUM(PaidAmount),0),
                      COALESCE(SUM(RemainingAmount),0), COUNT(*)
                      FROM Purchases
                      WHERE date(PurchaseDate) BETWEEN @from AND @to",
                    conn);
                purCmd.Parameters.AddWithValue("@from", from);
                purCmd.Parameters.AddWithValue("@to", to);
                using var pr = purCmd.ExecuteReader();
                double purTotal = 0, purPaid = 0, purRemaining = 0;
                int purCount = 0;
                if (pr.Read())
                {
                    purTotal = Convert.ToDouble(pr[0]);
                    purPaid = Convert.ToDouble(pr[1]);
                    purRemaining = Convert.ToDouble(pr[2]);
                    purCount = Convert.ToInt32(pr[3]);
                }
                pr.Close();

                using var expCmd = new SqliteCommand(
                    @"SELECT COALESCE(SUM(Amount),0), COUNT(*)
                      FROM Expenses
                      WHERE date(ExpenseDate) BETWEEN @from AND @to",
                    conn);
                expCmd.Parameters.AddWithValue("@from", from);
                expCmd.Parameters.AddWithValue("@to", to);
                using var er = expCmd.ExecuteReader();
                double expTotal = 0;
                int expCount = 0;
                if (er.Read())
                {
                    expTotal = Convert.ToDouble(er[0]);
                    expCount = Convert.ToInt32(er[1]);
                }
                er.Close();

                using var salCmd = new SqliteCommand(
                    @"SELECT COALESCE(SUM(Amount),0)
                      FROM Salaries
                      WHERE date(PaidDate) BETWEEN @from AND @to",
                    conn);
                salCmd.Parameters.AddWithValue("@from", from);
                salCmd.Parameters.AddWithValue("@to", to);
                double salTotal =
                    Convert.ToDouble(salCmd.ExecuteScalar());

                using var stockCmd = new SqliteCommand(
                    @"SELECT COALESCE(SUM(Stock * PurchasePrice),0)
                      FROM Medicines", conn);
                double stockValue =
                    Convert.ToDouble(stockCmd.ExecuteScalar());

                double netProfit =
                    salesTotal - purTotal - expTotal - salTotal;

                var list = new List<MasterReportItem>
                {
                    new MasterReportItem
                    {
                        Category = "Sales",
                        Count = salesCount,
                        TotalAmount = salesTotal,
                        PaidAmount = salesPaid,
                        Remaining = salesRemaining
                    },
                    new MasterReportItem
                    {
                        Category = "Purchases",
                        Count = purCount,
                        TotalAmount = purTotal,
                        PaidAmount = purPaid,
                        Remaining = purRemaining
                    },
                    new MasterReportItem
                    {
                        Category = "Expenses",
                        Count = expCount,
                        TotalAmount = expTotal,
                        PaidAmount = expTotal,
                        Remaining = 0
                    },
                    new MasterReportItem
                    {
                        Category = "Salaries",
                        Count = 0,
                        TotalAmount = salTotal,
                        PaidAmount = salTotal,
                        Remaining = 0
                    },
                    new MasterReportItem
                    {
                        Category = "Stock Value",
                        Count = 0,
                        TotalAmount = stockValue,
                        PaidAmount = 0,
                        Remaining = 0
                    },
                    new MasterReportItem
                    {
                        Category = "Net Profit",
                        Count = 0,
                        TotalAmount = netProfit,
                        PaidAmount = 0,
                        Remaining = 0
                    },
                };

                dgReport.ItemsSource = list;
                txtCard1Label.Text = "Total Sales";
                txtCard1Value.Text = $"AFN {salesTotal:N0}";
                txtCard2Label.Text = "Total Purchase";
                txtCard2Value.Text = $"AFN {purTotal:N0}";
                txtCard3Label.Text = "Total Expenses";
                txtCard3Value.Text = $"AFN {expTotal:N0}";
                txtCard4Label.Text = "Net Profit";
                txtCard4Value.Text = $"AFN {netProfit:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        #endregion

        #region Print

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgReport.ItemsSource == null)
                {
                    MessageBox.Show("Please select a report first!",
                        "No Report", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                string from = dpFrom.SelectedDate?
                    .ToString("dd/MM/yyyy")
                    ?? DateTime.Today.ToString("dd/MM/yyyy");
                string to = dpTo.SelectedDate?
                    .ToString("dd/MM/yyyy")
                    ?? DateTime.Today.ToString("dd/MM/yyyy");

                PrintCurrentReport(from, to);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print error: {ex.Message}");
            }
        }

        private void PrintCurrentReport(string from, string to)
        {
            var printDialog = new PrintDialog();

            printDialog.PrintTicket.PageOrientation =
                System.Printing.PageOrientation.Landscape;
            printDialog.PrintTicket.PageMediaSize =
                new System.Printing.PageMediaSize(
                    System.Printing.PageMediaSizeName.ISOA4);

            if (printDialog.ShowDialog() != true) return;

            printDialog.PrintTicket.PageOrientation =
                System.Printing.PageOrientation.Landscape;

            double pageW = printDialog.PrintableAreaWidth;
            double pageH = printDialog.PrintableAreaHeight;

            if (pageW < pageH)
            {
                double tmp = pageW;
                pageW = pageH;
                pageH = tmp;
            }

            var panel = BuildPrintPanel(from, to, pageW, pageH);

            panel.Measure(new Size(pageW, double.PositiveInfinity));
            panel.Arrange(new Rect(0, 0, pageW,
                panel.DesiredSize.Height));

            double naturalH = panel.DesiredSize.Height;
            double scale = naturalH > pageH
                ? pageH / naturalH : 1.0;

            panel.LayoutTransform =
                new ScaleTransform(scale, scale);

            panel.Measure(new Size(pageW, pageH));
            panel.Arrange(new Rect(new Size(pageW, pageH)));

            printDialog.PrintVisual(panel, txtReportTitle.Text);

            MessageBox.Show("Report printed successfully!",
                "Success", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private StackPanel BuildPrintPanel(string from, string to,
            double pageW, double pageH)
        {
            var panel = new StackPanel
            {
                Background = Brushes.White,
                Width = pageW,
                Margin = new Thickness(24)
            };

            // ===== HEADER =====
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            headerGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(180) });

            var leftStack = new StackPanel();

            leftStack.Children.Add(new TextBlock
            {
                Text = CompanyName,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(0x0D, 0x1B, 0x3E))
            });
            leftStack.Children.Add(new TextBlock
            {
                Text = CompanySubtitle,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(0x15, 0x65, 0xC0)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            leftStack.Children.Add(new TextBlock
            {
                Text = CompanyAddress,
                FontSize = 9,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 4, 0, 0)
            });
            leftStack.Children.Add(new TextBlock
            {
                Text = CompanyPhone,
                FontSize = 9,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 1, 0, 0)
            });
            leftStack.Children.Add(new TextBlock
            {
                Text = CompanyEmail,
                FontSize = 9,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(0x15, 0x65, 0xC0)),
                Margin = new Thickness(0, 1, 0, 0)
            });

            Grid.SetColumn(leftStack, 0);
            headerGrid.Children.Add(leftStack);

            var rightStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            rightStack.Children.Add(new TextBlock
            {
                Text = $"Date: {DateTime.Now:dd/MM/yyyy}",
                FontSize = 9,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right
            });
            rightStack.Children.Add(new TextBlock
            {
                Text = $"Time: {DateTime.Now:HH:mm}",
                FontSize = 9,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 2, 0, 0)
            });
            rightStack.Children.Add(new TextBlock
            {
                Text = "Printed by: Admin",
                FontSize = 9,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 2, 0, 0)
            });

            Grid.SetColumn(rightStack, 1);
            headerGrid.Children.Add(rightStack);

            panel.Children.Add(headerGrid);

            // Blue line
            panel.Children.Add(new Border
            {
                Height = 2,
                Background = new SolidColorBrush(
                    Color.FromRgb(0x15, 0x65, 0xC0)),
                Margin = new Thickness(0, 8, 0, 8)
            });

            // ===== REPORT TITLE =====
            panel.Children.Add(new TextBlock
            {
                Text = txtReportTitle.Text.ToUpper(),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(0x0D, 0x1B, 0x3E)),
                Margin = new Thickness(0, 0, 0, 2)
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"Period: {from}  to  {to}",
                FontSize = 9,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 8)
            });

            // ===== SUMMARY CARDS =====
            var cardsPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            void AddCard(string label, string value)
            {
                if (string.IsNullOrEmpty(label)) return;
                var card = new Border
                {
                    Background = new SolidColorBrush(
                        Color.FromRgb(0xF0, 0xF9, 0xFF)),
                    BorderBrush = new SolidColorBrush(
                        Color.FromRgb(0xBA, 0xE6, 0xFD)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 0, 8, 0),
                    MinWidth = 130
                };

                var cardContent = new StackPanel();
                cardContent.Children.Add(new TextBlock
                {
                    Text = label,
                    FontSize = 8,
                    Foreground = Brushes.Gray
                });
                cardContent.Children.Add(new TextBlock
                {
                    Text = value,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0x15, 0x65, 0xC0))
                });
                card.Child = cardContent;
                cardsPanel.Children.Add(card);
            }

            AddCard(txtCard1Label.Text, txtCard1Value.Text);
            AddCard(txtCard2Label.Text, txtCard2Value.Text);
            AddCard(txtCard3Label.Text, txtCard3Value.Text);
            AddCard(txtCard4Label.Text, txtCard4Value.Text);

            panel.Children.Add(cardsPanel);

            // ===== DATA GRID =====
            var printGrid = new DataGrid
            {
                ItemsSource = dgReport.ItemsSource,
                AutoGenerateColumns = true,
                IsReadOnly = true,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                FontSize = 9,
                FontFamily = new FontFamily("Segoe UI"),
                RowHeight = 22,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.LightGray,
                Background = Brushes.White,
                AlternatingRowBackground = new SolidColorBrush(
                    Color.FromRgb(0xF8, 0xF9, 0xFA)),
                HorizontalGridLinesBrush = Brushes.LightGray,
                VerticalGridLinesBrush = Brushes.LightGray,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var headerStyle = new Style(
                typeof(DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(
                Control.BackgroundProperty,
                new SolidColorBrush(
                    Color.FromRgb(0x15, 0x65, 0xC0))));
            headerStyle.Setters.Add(new Setter(
                Control.ForegroundProperty,
                Brushes.White));
            headerStyle.Setters.Add(new Setter(
                Control.FontWeightProperty,
                FontWeights.Bold));
            headerStyle.Setters.Add(new Setter(
                Control.FontSizeProperty, 9.5));
            headerStyle.Setters.Add(new Setter(
                Control.HeightProperty, 26.0));
            headerStyle.Setters.Add(new Setter(
                Control.PaddingProperty,
                new Thickness(6, 0, 6, 0)));
            headerStyle.Setters.Add(new Setter(
                Control.FontFamilyProperty,
                new FontFamily("Segoe UI")));

            printGrid.ColumnHeaderStyle = headerStyle;

            panel.Children.Add(printGrid);

            // ===== FOOTER =====
            panel.Children.Add(new Border
            {
                Height = 1,
                Background = Brushes.LightGray,
                Margin = new Thickness(0, 8, 0, 4)
            });

            var footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            footerGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

            var footerLeft = new TextBlock
            {
                Text = $"Printed: {DateTime.Now:dd/MM/yyyy HH:mm}" +
                       "  |  Admin",
                FontSize = 8,
                Foreground = Brushes.Gray
            };

            var footerRight = new TextBlock
            {
                Text = CompanyDeveloper,
                FontSize = 8,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(footerLeft, 0);
            Grid.SetColumn(footerRight, 1);
            footerGrid.Children.Add(footerLeft);
            footerGrid.Children.Add(footerRight);
            panel.Children.Add(footerGrid);

            return panel;
        }

        #endregion

        #region Utilities

        private string FormatDate(string rawDate)
        {
            if (DateTime.TryParse(rawDate, out DateTime d))
                return d.ToString("dd/MM/yyyy");
            return rawDate;
        }

        #endregion

        #region Model Classes

        public class SalesReportItem
        {
            public string VoucherNo { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public double TotalAmount { get; set; }
            public double PaidAmount { get; set; }
            public double RemainingAmount { get; set; }
            public string SaleDate { get; set; } = "";
        }

        public class PurchaseReportItem
        {
            public string VoucherNo { get; set; } = "";
            public string SupplierName { get; set; } = "";
            public double TotalAmount { get; set; }
            public double PaidAmount { get; set; }
            public double RemainingAmount { get; set; }
            public string PurchaseDate { get; set; } = "";
        }

        public class StockReportItem
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string GenericName { get; set; } = "";
            public string Company { get; set; } = "";
            public double PurchasePrice { get; set; }
            public double SalePrice { get; set; }
            public int Stock { get; set; }
            public string ExpiryDate { get; set; } = "";
        }

        public class ExpenseReportItem
        {
            public string Title { get; set; } = "";
            public string Category { get; set; } = "";
            public double Amount { get; set; }
            public string ExpenseDate { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        public class SalaryReportItem
        {
            public string EmployeeName { get; set; } = "";
            public double Amount { get; set; }
            public string Month { get; set; } = "";
            public string PaidDate { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        public class DebtReportItem
        {
            public string CustomerName { get; set; } = "";
            public string Phone { get; set; } = "";
            public double TotalAmount { get; set; }
            public double PaidAmount { get; set; }
            public double RemainingAmount { get; set; }
            public string DebtDate { get; set; } = "";
        }

        public class MasterReportItem
        {
            public string Category { get; set; } = "";
            public int Count { get; set; }
            public double TotalAmount { get; set; }
            public double PaidAmount { get; set; }
            public double Remaining { get; set; }
        }

        #endregion
    }
}