using AppForQA;
using AppForQA.Context;
using AppForQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace IntegrationTests
{
    [TestClass]
    [DoNotParallelize]
    public class IntegrationTests
    {
        private const string TestFile = "test_integration.txt";

        [TestInitialize]
        public void Setup()
        {
            // Удаляем тестовый файл если существует
            if (File.Exists(TestFile))
            {
                File.Delete(TestFile);
            }

            // Очищаем БД
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            db.Database.ExecuteSqlRaw("DELETE FROM \"People\"");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(TestFile))
            {
                File.Delete(TestFile);
            }
        }

        /// <summary>
        /// Тест 1: Экспорт данных из БД в файл
        /// </summary>
        [TestMethod]
        public void Export_CreatesFile_WithData()
        {
            // Arrange: добавляем данные в БД
            using (var db = new AppDbContext())
            {
                db.People.Add(new Person { FullName = "Иван Иванов" });
                db.SaveChanges();
            }

            // Act: экспортируем
            var exporter = new DataExporter();
            exporter.Export(TestFile);

            // Assert: файл создан и содержит данные
            Assert.IsTrue(File.Exists(TestFile));

            var lines = File.ReadAllLines(TestFile);
            Assert.HasCount(1, lines);
            Assert.Contains("Иван Иванов", lines[0]);
        }

        /// <summary>
        /// Тест 2: Импорт данных из файла в БД
        /// </summary>
        [TestMethod]
        public void Import_AddsData_ToDatabase()
        {
            // Arrange: создаём файл с данными
            File.WriteAllLines(TestFile, new[] { "1;Петр Петров" });

            // Act: импортируем
            var importer = new DataImporter();
            importer.Import(TestFile);

            // Assert: данные появились в БД
            using var db = new AppDbContext();
            Assert.IsTrue(db.People.Any(p => p.FullName == "Петр Петров"));
        }

        /// <summary>
        /// Тест 3: Полный цикл — экспорт затем импорт
        /// </summary>
        [TestMethod]
        public void FullCycle_ExportThenImport_Works()
        {
            // Arrange: добавляем данные в БД
            using (var db = new AppDbContext())
            {
                db.People.Add(new Person { FullName = "Анна Смирнова" });
                db.SaveChanges();
            }

            // Act 1: экспортируем в файл
            var exporter = new DataExporter();
            exporter.Export(TestFile);

            // Очищаем БД
            using (var db = new AppDbContext())
            {
                db.Database.ExecuteSqlRaw("DELETE FROM \"People\"");
            }

            // Act 2: импортируем из файла обратно
            var importer = new DataImporter();
            importer.Import(TestFile);

            // Assert: данные восстановлены
            using var db2 = new AppDbContext();
            Assert.IsTrue(db2.People.Any(p => p.FullName == "Анна Смирнова"));
        }

        /// <summary>
        /// Тест 4: Проверка импорта через метод с out параметром
        /// </summary>
        [TestMethod]
        public void TestFileImport()
        {
            // Arrange: создаём файл с данными
            var expectedContent = "1;Мария Козлова\r\n2;Алексей Волков";
            File.WriteAllText(TestFile, expectedContent);

            // Act: импортируем с получением содержимого
            var importer = new DataImporter();
            importer.Import(TestFile, out string fileContent);

            // Assert: содержимое прочитано корректно
            Assert.AreEqual(expectedContent, fileContent);

            // И данные добавлены в БД
            using var db = new AppDbContext();
            Assert.IsTrue(db.People.Any(p => p.FullName == "Мария Козлова"));
            Assert.IsTrue(db.People.Any(p => p.FullName == "Алексей Волков"));
        }
    }
}
