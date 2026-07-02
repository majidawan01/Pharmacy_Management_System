using System.Windows;
using System.Windows.Input;
using Pharmacy_Management_System.Views;

namespace Pharmacy_Management_System.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };

            // Dashboard default load
            MainFrame.Navigate(new DashboardPage());
            txtPageTitle.Text = "Dashboard";
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null) return;

            string page = btn.Tag?.ToString() ?? "";
            txtPageTitle.Text = page;

            switch (page)
            {
                case "Dashboard":
                    MainFrame.Navigate(new DashboardPage());
                    break;
                case "Medicines":
                     MainFrame.Navigate(new MedicinesPage());
                    break;
                case "Purchase":
                     MainFrame.Navigate(new PurchasePage());
                    break;
                case "Sales":
                     MainFrame.Navigate(new SalesPage());
                    break;
                case "Expenses":
                     MainFrame.Navigate(new ExpensesPage());
                    break;
                case "Employees":
                     MainFrame.Navigate(new EmployeesPage());
                    break;
                case "Salary":
                     MainFrame.Navigate(new SalaryPage());
                    break;
                case "Debts":
                     MainFrame.Navigate(new DebtsPage());
                    break;
                case "Returns":
                     MainFrame.Navigate(new ReturnsPage());
                    break;
                case "Reports":
                     MainFrame.Navigate(new ReportsPage());
                    break;
                case "Settings":
                     MainFrame.Navigate(new SettingsPage());
                    break;
                case "Barcode":
                    var barcodeWindow = new BarcodeWindow();
                    barcodeWindow.Owner = this;
                    barcodeWindow.ShowDialog();
                    break;


               case "Accounts":
                    MainFrame.Navigate(new AccountsPage());
                    break;

                case "Logs":
                    MainFrame.Navigate(new LogsPage());
                    break;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        public void UpdateLanguage()
        {
            try
            {
                // Page title update
                txtPageTitle.Text = Helpers.AppLanguage.Get(txtPageTitle.Text);

                // Reload current page
                string current = txtPageTitle.Text;
                MainFrame.Navigate(null);

                switch (Helpers.AppLanguage.CurrentLanguage)
                {
                    case "ps":
                    case "da":
                    case "ur":
                    case "ar":
                        this.FlowDirection = FlowDirection.RightToLeft;
                        break;
                    default:
                        this.FlowDirection = FlowDirection.LeftToRight;
                        break;
                }

                // Sidebar text update karo directly
                UpdateSidebarText();
            }
            catch { }
        }

        private void UpdateSidebarText()
        {
            try
            {
                // Har button ke andar TextBlock find karke update karo
                SetButtonText(btnDashboard, "📊", Helpers.AppLanguage.Get("Dashboard"));
                SetButtonText(btnMedicines, "💉", Helpers.AppLanguage.Get("Medicines"));
                SetButtonText(btnPurchase, "🛒", Helpers.AppLanguage.Get("Purchase"));
                SetButtonText(btnSales, "💰", Helpers.AppLanguage.Get("Sales"));
                SetButtonText(btnExpenses, "💸", Helpers.AppLanguage.Get("Expenses"));
                SetButtonText(btnEmployees, "👥", Helpers.AppLanguage.Get("Employees"));
                SetButtonText(btnSalary, "💵", Helpers.AppLanguage.Get("Salary"));
                SetButtonText(btnDebts, "📋", Helpers.AppLanguage.Get("Debts"));
                SetButtonText(btnReturns, "↩️", Helpers.AppLanguage.Get("Returns"));
                SetButtonText(btnAccounts, "📒", Helpers.AppLanguage.Get("Accounts"));
                SetButtonText(btnLogs, "📋", Helpers.AppLanguage.Get("Logs"));
                SetButtonText(btnReports, "📈", Helpers.AppLanguage.Get("Reports"));
                SetButtonText(btnBarcode, "🔖", Helpers.AppLanguage.Get("Barcode"));
                SetButtonText(btnSettings, "⚙️", Helpers.AppLanguage.Get("Settings"));
            }
            catch { }
        }

        private void SetButtonText(System.Windows.Controls.Button btn, string emoji, string text)
        {
            try
            {
                if (btn == null) return;
                btn.Content = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Children =
            {
                new System.Windows.Controls.TextBlock
                {
                    Text = emoji,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center
                },
                new System.Windows.Controls.TextBlock
                {
                    Text = text,
                    FontSize = 13,
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(12, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
                };
            }
            catch { }
        }
        private void UpdateNavButton(System.Windows.Controls.Button btn, string key)
        {
            try
            {
                if (btn == null) return;
                var stackPanel = ((btn.Template.FindName("navBorder",
                    btn) as System.Windows.Controls.Border)
                    ?.Child as System.Windows.Controls.StackPanel);

                if (stackPanel != null && stackPanel.Children.Count > 1)
                {
                    if (stackPanel.Children[1] is System.Windows.Controls.TextBlock tb)
                        tb.Text = Helpers.AppLanguage.Get(key);
                }
            }
            catch { }
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                btnMaximize.Content = "□";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                btnMaximize.Content = "❐";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}