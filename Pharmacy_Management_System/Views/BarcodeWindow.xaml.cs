using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace Pharmacy_Management_System.Views
{
    public partial class BarcodeWindow : Window
    {
        private List<MedicineBarcode> _medicines = new();
        private BitmapSource? _currentBarcode = null;

        public BarcodeWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                LoadMedicines();
                txtMedicineSearch.Focus();
            };
        }

        private void LoadMedicines()
        {
            try
            {
                _medicines = new List<MedicineBarcode>();
                using var conn = Database.DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SqliteCommand(
                    "SELECT Id, Name, Code, SalePrice FROM Medicines ORDER BY Name",
                    conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _medicines.Add(new MedicineBarcode
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"]?.ToString() ?? "",
                        Code = reader["Code"]?.ToString() ?? "",
                        SalePrice = Convert.ToDouble(reader["SalePrice"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading medicines: {ex.Message}");
            }
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
                        SelectBarcodeMedicine();
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
                    SelectBarcodeMedicine();
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

                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    SelectBarcodeMedicine();
            }
            catch { }
        }

        private void SelectBarcodeMedicine()
        {
            try
            {
                if (lstMedicineSuggestions.SelectedItem == null) return;

                string name = lstMedicineSuggestions.SelectedItem.ToString() ?? "";
                var med = _medicines.FirstOrDefault(m => m.Name == name);

                if (med != null)
                {
                    txtMedicineSearch.Text = med.Name;
                    txtBarcodeText.Text = string.IsNullOrEmpty(med.Code)
                        ? med.Id.ToString() : med.Code;
                    txtMedicineName.Text = med.Name;
                    txtMedicinePrice.Text = $"Price: AFN {med.SalePrice:N0}";
                    lstMedicineSuggestions.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateBarcode();
        }

        private void GenerateBarcode()
        {
            try
            {
                string text = txtBarcodeText.Text.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    txtError.Text = "Please select a medicine first!";
                    txtError.Visibility = Visibility.Visible;
                    return;
                }

                txtError.Visibility = Visibility.Collapsed;

                string type = (cmbBarcodeType.SelectedItem as ComboBoxItem)
                    ?.Content?.ToString() ?? "CODE_128";

                BarcodeFormat format = type switch
                {
                    "QR_CODE" => BarcodeFormat.QR_CODE,
                    "CODE_39" => BarcodeFormat.CODE_39,
                    _ => BarcodeFormat.CODE_128
                };

                var writer = new BarcodeWriterPixelData
                {
                    Format = format,
                    Options = new EncodingOptions
                    {
                        Width = 300,
                        Height = 150,
                        Margin = 10
                    }
                };

                var pixelData = writer.Write(text);

                var bitmap = BitmapSource.Create(
                    pixelData.Width,
                    pixelData.Height,
                    96, 96,
                    System.Windows.Media.PixelFormats.Bgr32,
                    null,
                    pixelData.Pixels,
                    pixelData.Width * 4);

                _currentBarcode = bitmap;
                imgBarcode.Source = bitmap;
                txtBarcodeLabel.Text = text;
            }
            catch (Exception ex)
            {
                txtError.Text = $"Error: {ex.Message}";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBarcode == null)
            {
                MessageBox.Show("Please generate a barcode first!",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (!int.TryParse(txtCopies.Text, out int copies) || copies <= 0)
                    copies = 1;

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    var printPanel = new WrapPanel();
                    printPanel.Width = printDialog.PrintableAreaWidth;

                    for (int i = 0; i < copies; i++)
                    {
                        var barcodePanel = new StackPanel
                        {
                            Margin = new Thickness(5),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        var img = new System.Windows.Controls.Image
                        {
                            Source = _currentBarcode,
                            Width = 150,
                            Height = 75
                        };

                        var label = new TextBlock
                        {
                            Text = txtBarcodeText.Text,
                            FontSize = 9,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 2, 0, 0)
                        };

                        var name = new TextBlock
                        {
                            Text = txtMedicineName.Text,
                            FontSize = 9,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            MaxWidth = 150
                        };

                        var price = new TextBlock
                        {
                            Text = txtMedicinePrice.Text,
                            FontSize = 9,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        barcodePanel.Children.Add(img);
                        barcodePanel.Children.Add(label);
                        barcodePanel.Children.Add(name);
                        barcodePanel.Children.Add(price);
                        printPanel.Children.Add(barcodePanel);
                    }

                    printPanel.Measure(new System.Windows.Size(
                        printDialog.PrintableAreaWidth,
                        printDialog.PrintableAreaHeight));
                    printPanel.Arrange(new Rect(new System.Windows.Size(
                        printDialog.PrintableAreaWidth,
                        printDialog.PrintableAreaHeight)));

                    printDialog.PrintVisual(printPanel, "Barcode Print");
                    MessageBox.Show($"{copies} barcode(s) printed successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print error: {ex.Message}");
            }
        }
    }

    public class MedicineBarcode
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public double SalePrice { get; set; }
    }
}