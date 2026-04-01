using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace CRCChecker
{
    public partial class MainWindow : Window
    {
        private string? selectedFilePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select a file to calculate CRC32 checksum",
                Filter = "All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                FilePathTextBox.Text = selectedFilePath;
                DisplayFileInfo();
            }
        }

        //private void CalculateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
        //    {
        //        MessageBox.Show("Please select a valid file first.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    CalculateCRC32();
        //}

        private void DisplayFileInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(selectedFilePath)) return;

                var fileInfo = new FileInfo(selectedFilePath);
                FileNameText.Text = fileInfo.Name;
                FileSizeText.Text = FormatFileSize(fileInfo.Length);
                LastModifiedText.Text = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                CalculateCRC32();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file information: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateCRC32()
        {
            try
            {
                BrowseButton.IsEnabled = false;

                var startTime = DateTime.Now;
                
                // Run calculation in background to keep UI responsive
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        string crc32Checksum = new CRC(CRCCode.CRC32).FindCRC32(selectedFilePath).ToString("X2");
                        var endTime = DateTime.Now;
                        var duration = endTime - startTime;

                        // Update UI on main thread
                        Dispatcher.Invoke(() =>
                        {
                            CRC32Text.Text = crc32Checksum;
                            BrowseButton.IsEnabled = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error calculating CRC32: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            BrowseButton.IsEnabled = true;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting CRC32 calculation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BrowseButton.IsEnabled = true;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
