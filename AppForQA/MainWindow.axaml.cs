using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace AppForQA
{
    public partial class MainWindow : Window
    {
        private string path = "data.txt";

        public MainWindow()
        {
            InitializeComponent();

            // Импорт данных из файла при загрузке формы
            try
            {
                var importer = new DataImporter();
                importer.Import(path, out string data);
                ImportedDataText.Text = data;
            }
            catch
            {
                ImportedDataText.Text = "Файл данных не найден";
            }
        }

        private void Export_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var exporter = new DataExporter();
                exporter.Export(path);

                StatusText.Text = "Экспорт выполнен!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void Import_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var importer = new DataImporter();
                importer.Import(path, out string data);
                ImportedDataText.Text = data;

                StatusText.Text = "Импорт выполнен!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }
    }
}
