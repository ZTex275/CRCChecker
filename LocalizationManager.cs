using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;

namespace CRCChecker
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static LocalizationManager _instance;
        
        private LocalizationManager()
        {
            LoadLanguage();
        }
        
        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        private SupportedLanguage _currentLanguage;
        public SupportedLanguage CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    SaveLanguage();
                    OnPropertyChanged(nameof(CurrentLanguage));
                    UpdateAllStrings();
                }
            }
        }

        public string WindowTitle => CurrentLanguage switch
        {
            SupportedLanguage.English => "CRC32 File Checker",
            SupportedLanguage.Russian => "CRC32 Проверка файлов"
        };

        public string TitleText => CurrentLanguage switch
        {
            SupportedLanguage.English => "CRC32 File Checker",
            SupportedLanguage.Russian => "CRC32 Проверка файлов"
        };

        public string NoFileSelected => CurrentLanguage switch
        {
            SupportedLanguage.English => "No file selected",
            SupportedLanguage.Russian => "Файл не выбран"
        };

        public string BrowseButton => CurrentLanguage switch
        {
            SupportedLanguage.English => "Browse...",
            SupportedLanguage.Russian => "Обзор..."
        };

        public string FileNameLabel => CurrentLanguage switch
        {
            SupportedLanguage.English => "File Name:",
            SupportedLanguage.Russian => "Имя файла:"
        };

        public string FileSizeLabel => CurrentLanguage switch
        {
            SupportedLanguage.English => "File Size:",
            SupportedLanguage.Russian => "Размер файла:"
        };

        public string LastModifiedLabel => CurrentLanguage switch
        {
            SupportedLanguage.English => "Last Modified:",
            SupportedLanguage.Russian => "Последнее изменение:"
        };

        public string CRC32Label => CurrentLanguage switch
        {
            SupportedLanguage.English => "CRC32:",
            SupportedLanguage.Russian => "CRC32:"
        };

        public string FileVersionLabel => CurrentLanguage switch
        {
            SupportedLanguage.English => "File Version:",
            SupportedLanguage.Russian => "Версия файла:"
        };

        public string SelectFileTitle => CurrentLanguage switch
        {
            SupportedLanguage.English => "Select a file to calculate CRC32 checksum",
            SupportedLanguage.Russian => "Выберите файл для расчета контрольной суммы CRC32"
        };

        public string AllFilesFilter => CurrentLanguage switch
        {
            SupportedLanguage.English => "All files (*.*)|*.*",
            SupportedLanguage.Russian => "Все файлы (*.*)|*.*"
        };

        public string NoFileSelectedMessage => CurrentLanguage switch
        {
            SupportedLanguage.English => "Please select a valid file first.",
            SupportedLanguage.Russian => "Пожалуйста, сначала выберите действительный файл."
        };

        public string NoFileSelectedTitle => CurrentLanguage switch
        {
            SupportedLanguage.English => "No File Selected",
            SupportedLanguage.Russian => "Файл не выбран"
        };

        public string ErrorReadingFile => CurrentLanguage switch
        {
            SupportedLanguage.English => "Error reading file information: ",
            SupportedLanguage.Russian => "Ошибка чтения информации о файле: "
        };

        public string ErrorTitle => CurrentLanguage switch
        {
            SupportedLanguage.English => "Error",
            SupportedLanguage.Russian => "Ошибка"
        };

        public string ErrorCalculatingCRC => CurrentLanguage switch
        {
            SupportedLanguage.English => "Error calculating CRC32: ",
            SupportedLanguage.Russian => "Ошибка расчета CRC32: "
        };

        public string ErrorStartingCalculation => CurrentLanguage switch
        {
            SupportedLanguage.English => "Error starting CRC32 calculation: ",
            SupportedLanguage.Russian => "Ошибка запуска расчета CRC32: "
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action LanguageChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateAllStrings()
        {
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(NoFileSelected));
            OnPropertyChanged(nameof(BrowseButton));
            OnPropertyChanged(nameof(FileNameLabel));
            OnPropertyChanged(nameof(FileSizeLabel));
            OnPropertyChanged(nameof(LastModifiedLabel));
            OnPropertyChanged(nameof(CRC32Label));
            OnPropertyChanged(nameof(FileVersionLabel));
            OnPropertyChanged(nameof(SelectFileTitle));
            OnPropertyChanged(nameof(AllFilesFilter));
            OnPropertyChanged(nameof(NoFileSelectedMessage));
            OnPropertyChanged(nameof(NoFileSelectedTitle));
            OnPropertyChanged(nameof(ErrorReadingFile));
            OnPropertyChanged(nameof(ErrorTitle));
            OnPropertyChanged(nameof(ErrorCalculatingCRC));
            OnPropertyChanged(nameof(ErrorStartingCalculation));
            
            LanguageChanged?.Invoke();
        }
        
        private void SaveLanguage()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\CRCChecker"))
                {
                    key?.SetValue("Language", _currentLanguage.ToString());
                }
            }
            catch (Exception)
            {
                // Silently fail if registry access is denied
            }
        }
        
        private void LoadLanguage()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\CRCChecker"))
                {
                    if (key != null)
                    {
                        string languageString = key.GetValue("Language") as string;
                        if (Enum.TryParse<SupportedLanguage>(languageString, out var savedLanguage))
                        {
                            _currentLanguage = savedLanguage;
                        }
                        else
                        {
                            _currentLanguage = SupportedLanguage.Russian; // Default language
                        }
                    }
                    else
                    {
                        _currentLanguage = SupportedLanguage.Russian; // Default language
                    }
                }
            }
            catch (Exception)
            {
                _currentLanguage = SupportedLanguage.Russian; // Default language on error
            }
        }
    }

    public enum SupportedLanguage
    {
        English,
        Russian
    }
}
