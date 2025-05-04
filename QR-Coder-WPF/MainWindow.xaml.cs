using Microsoft.Win32;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QR_Coder_WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnGenerate.Click += BtnGenerate_Click;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string input = txtInput.Text;
            string qrData = string.Empty;

            if (base64Option.IsChecked == true)
            {
                // Option 1: Convert Base64 to text
                try
                {
                    byte[] base64Bytes = Convert.FromBase64String(input);
                    qrData = Encoding.UTF8.GetString(base64Bytes);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid Base64 string.");
                    return;
                }
            }
            else if (textOption.IsChecked == true)
            {
                // Option 2: Use text as is
                qrData = input;
            }
            else if (wifiOption.IsChecked == true)
            {
                // Option 3: Create Wi-Fi QR Code
                var parts = input.Split(';');
                if (parts.Length < 2)
                {
                    MessageBox.Show("Invalid Wi-Fi input. Please provide in format 'SSID;Password' without qoutes.");
                    return;
                }
                qrData = $"WIFI:T:WPA;S:{input.Split(';')[0]};P:{input.Split(';')[1]};;";
            }

            GenerateQRCode(qrData);
        }

        private void GenerateQRCode(string qrData)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    Bitmap qrCodeImage = qrCode.GetGraphic(20);

                    // Convert Bitmap to BitmapImage for WPF Image control
                    pictureBoxQRCode.Source = ConvertBitmapToBitmapImage(qrCodeImage);
                }
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (pictureBoxQRCode.Source != null)
            {
                // Convert BitmapImage to Bitmap for clipboard
                BitmapSource bitmapSource = (BitmapSource)pictureBoxQRCode.Source;
                Clipboard.SetImage(bitmapSource);
                MessageBox.Show("QR Code copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No QR Code to copy. Please generate one first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (pictureBoxQRCode.Source != null)
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image|*.png",
                    Title = "Save QR Code",
                    FileName = "QRCode.png" // Default file name
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // Convert BitmapImage to Bitmap and save
                        BitmapSource bitmapSource = (BitmapSource)pictureBoxQRCode.Source;
                        using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                            encoder.Save(fileStream);
                        }

                        MessageBox.Show("QR Code saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No QR Code to save. Please generate one first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true // Ensures the URL opens in the default browser
            });
            e.Handled = true;
        }

    }
}
