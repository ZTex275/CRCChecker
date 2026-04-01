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
        private LocalizationManager localizationManager;

        public MainWindow()
        {
            InitializeComponent();
            localizationManager = LocalizationManager.Instance;
            localizationManager.LanguageChanged += OnLanguageChanged;
            DataContext = localizationManager;
            
            // Set initial button states
            UpdateLanguageButtonStates();
        }

        private void OnLanguageChanged()
        {
            // Update UI elements that don't use binding
            UpdateLanguageButtonStates();
            
            // Force update of bindings
            if (selectedFilePath != null)
            {
                FilePathTextBox.Text = selectedFilePath;
            }
            else
            {
                FilePathTextBox.Text = localizationManager.NoFileSelected;
            }
        }

        private void UpdateLanguageButtonStates()
        {
            if (EnglishButton != null && RussianButton != null)
            {
                EnglishButton.Style = localizationManager.CurrentLanguage == SupportedLanguage.English 
                    ? FindResource("ActiveLanguageButtonStyle") as Style 
                    : FindResource("LanguageButtonStyle") as Style;
                    
                RussianButton.Style = localizationManager.CurrentLanguage == SupportedLanguage.Russian 
                    ? FindResource("ActiveLanguageButtonStyle") as Style 
                    : FindResource("LanguageButtonStyle") as Style;
            }
        }

        private void EnglishButton_Click(object sender, RoutedEventArgs e)
        {
            localizationManager.CurrentLanguage = SupportedLanguage.English;
        }

        private void RussianButton_Click(object sender, RoutedEventArgs e)
        {
            localizationManager.CurrentLanguage = SupportedLanguage.Russian;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = localizationManager.SelectFileTitle,
                Filter = localizationManager.AllFilesFilter,
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
                MessageBox.Show($"{localizationManager.ErrorReadingFile}{ex.Message}", localizationManager.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
                            MessageBox.Show($"{localizationManager.ErrorCalculatingCRC}{ex.Message}", localizationManager.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            BrowseButton.IsEnabled = true;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{localizationManager.ErrorStartingCalculation}{ex.Message}", localizationManager.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
