using Avalonia.Controls;
using Avalonia.Interactivity;
using AppForQA.Models;
using System;

namespace AppForQA
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Загружаем данные из БД при открытии окна
            LoadDataFromDatabase();
        }

        /// <summary>
        /// Загружает все записи из БД и отображает их.
        /// </summary>
        private void LoadDataFromDatabase()
        {
            try
            {
                var importer = new DataImporter();
                importer.Import(out string data);
                ImportedDataText.Text = data;

                int count = importer.GetRecordCount();
                if (count >= 0)
                {
                    StatusText.Text = $"Загружено записей: {count}";
                }
                else
                {
                    StatusText.Text = "Ошибка подсчёта записей";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка загрузки: {ex.Message}";
                ImportedDataText.Text = "Не удалось загрузить данные";
            }
        }

        /// <summary>
        /// Добавляет новую запись в БД (экспорт).
        /// </summary>
        private void AddRecord_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                string? name = NameInput.Text?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    StatusText.Text = "Введите ФИО!";
                    return;
                }

                // Экспортируем (добавляем) запись в БД — передаём только строку
                var exporter = new DataExporter();
                bool success = exporter.Export(name, out string errorMessage);

                if (success)
                {
                    NameInput.Text = "";
                    StatusText.Text = $"Запись '{name}' добавлена в БД!";
                    // Обновляем отображение
                    LoadDataFromDatabase();
                }
                else
                {
                    StatusText.Text = $"Ошибка: {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Непредвиденная ошибка: {ex.Message}";
            }
        }

        /// <summary>
        /// Обновляет список записей из БД (импорт).
        /// </summary>
        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                LoadDataFromDatabase();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка обновления: {ex.Message}";
            }
        }
    }
}
