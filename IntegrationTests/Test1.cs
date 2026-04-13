using AppForQA;
using AppForQA.Context;
using AppForQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTests
{
    [TestClass]
    [DoNotParallelize]
    public class IntegrationTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Очищаем БД перед каждым тестом
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            db.People.ExecuteDelete();
        }

        #region Позитивные тесты

        /// <summary>
        /// Тест 1: Экспорт данных — добавление имён в БД через DataExporter.
        /// </summary>
        [TestMethod]
        public void TestExportToDatabase()
        {
            // Arrange
            var names = new List<string> { "Иван Иванов", "Петр Петров" };

            // Act
            var exporter = new DataExporter();
            bool result = exporter.Export(names, out string errorMessage);

            // Assert
            Assert.IsTrue(result, $"Экспорт не удался: {errorMessage}");
            Assert.IsTrue(string.IsNullOrEmpty(errorMessage));

            using var db = new AppDbContext();
            Assert.HasCount(2, db.People);
            Assert.IsTrue(db.People.Any(p => p.FullName == "Иван Иванов"));
            Assert.IsTrue(db.People.Any(p => p.FullName == "Петр Петров"));
        }

        /// <summary>
        /// Тест 2: Импорт данных — чтение записей из БД через DataImporter.
        /// </summary>
        [TestMethod]
        public void TestImportFromDatabase()
        {
            // Arrange
            using (var db = new AppDbContext())
            {
                db.People.Add(new Person { FullName = "Анна Смирнова" });
                db.People.Add(new Person { FullName = "Мария Козлова" });
                db.SaveChanges();
            }

            // Act
            var importer = new DataImporter();
            var result = importer.Import();

            // Assert
            Assert.HasCount(2, result);
            Assert.IsTrue(result.Any(p => p.FullName == "Анна Смирнова"));
            Assert.IsTrue(result.Any(p => p.FullName == "Мария Козлова"));
        }

        /// <summary>
        /// Тест 3: Полный цикл — экспорт затем импорт.
        /// </summary>
        [TestMethod]
        public void TestFullCycle_ExportThenImport()
        {
            // Arrange
            var namesToExport = new List<string> { "Алексей Волков", "Елена Новикова" };

            // Act 1: экспорт
            var exporter = new DataExporter();
            bool exportOk = exporter.Export(namesToExport, out _);
            Assert.IsTrue(exportOk);

            // Act 2: импорт
            var importer = new DataImporter();
            var importedData = importer.Import();

            // Assert
            Assert.HasCount(2, importedData);
            Assert.IsTrue(importedData.Any(p => p.FullName == "Алексей Волков"));
            Assert.IsTrue(importedData.Any(p => p.FullName == "Елена Новикова"));
        }

        /// <summary>
        /// Тест 4: Проверка импорта с out параметром (форматированная строка).
        /// </summary>
        [TestMethod]
        public void TestImportWithOutParameter()
        {
            // Arrange
            using (var db = new AppDbContext())
            {
                db.People.Add(new Person { FullName = "Дмитрий Соколов" });
                db.SaveChanges();
            }

            // Act
            var importer = new DataImporter();
            importer.Import(out string formattedData);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(formattedData));
            Assert.Contains("Дмитрий Соколов", formattedData);
            // Проверяем, что есть форматирование с номером (цифра и точка)
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(formattedData, @"\d\."));
        }

        /// <summary>
        /// Тест 5: Проверка фильтрации при импорте.
        /// </summary>
        [TestMethod]
        public void TestImportWithFilter()
        {
            // Arrange
            using (var db = new AppDbContext())
            {
                db.People.AddRange(
                    new Person { FullName = "Иван Иванов" },
                    new Person { FullName = "Петр Иванов" },
                    new Person { FullName = "Мария Петрова" }
                );
                db.SaveChanges();
            }

            // Act
            var importer = new DataImporter();
            var filtered = importer.ImportByFilter("Иванов");

            // Assert
            Assert.HasCount(2, filtered);
            Assert.IsTrue(filtered.All(p => p.FullName!.Contains("Иванов")));
        }

        /// <summary>
        /// Тест 6: Проверка защиты от дубликатов при экспорте.
        /// </summary>
        [TestMethod]
        public void TestExportPreventsDuplicates()
        {
            var exporter = new DataExporter();

            // Act: добавляем одно и то же имя дважды
            bool ok1 = exporter.Export("Олег Морозов", out _);
            bool ok2 = exporter.Export("Олег Морозов", out _);

            // Assert: оба вызова успешны, но в БД одна запись
            Assert.IsTrue(ok1);
            Assert.IsTrue(ok2);

            using var db = new AppDbContext();
            Assert.HasCount(1, db.People.Where(p => p.FullName == "Олег Морозов"));
        }

        /// <summary>
        /// Тест 7: Проверка GetRecordCount.
        /// </summary>
        [TestMethod]
        public void TestRecordCount()
        {
            // Arrange
            using (var db = new AppDbContext())
            {
                db.People.AddRange(
                    new Person { FullName = "Тест 1" },
                    new Person { FullName = "Тест 2" },
                    new Person { FullName = "Тест 3" }
                );
                db.SaveChanges();
            }

            // Act
            var exporter = new DataExporter();
            var importer = new DataImporter();
            int exportCount = exporter.GetRecordCount();
            int importCount = importer.GetRecordCount();

            // Assert
            Assert.AreEqual(3, exportCount);
            Assert.AreEqual(3, importCount);
        }

        #endregion

        #region Негативные тесты

        /// <summary>
        /// Негативный тест 1: Экспорт null списка — должна быть ошибка.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ExportNullList_ReturnsError()
        {
            var exporter = new DataExporter();

            // Act
            List<string>? nullList = null;
            bool result = exporter.Export(nullList, out string errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
            Assert.Contains("null", errorMessage);
        }

        /// <summary>
        /// Негативный тест 2: Экспорт пустой строки.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ExportEmptyString_ReturnsError()
        {
            var exporter = new DataExporter();

            // Act
            bool result = exporter.Export("", out string errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
        }

        /// <summary>
        /// Негативный тест 3: Экспорт строки из одних пробелов.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ExportWhitespaceString_ReturnsError()
        {
            var exporter = new DataExporter();

            // Act
            bool result = exporter.Export("   ", out string errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
        }

        /// <summary>
        /// Негативный тест 4: Экспорт null-строки.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ExportNullString_ReturnsError()
        {
            var exporter = new DataExporter();

            // Act
            string? nullName = null;
            bool result = exporter.Export(nullName, out string errorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
            Assert.Contains("null", errorMessage);
        }

        /// <summary>
        /// Негативный тест 5: Экспорт списка с пустыми строками — пропускаются.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ExportListWithEmptyNames_SkipsEmpty()
        {
            var names = new List<string> { "", "   ", "Валидное Имя", null! };

            var exporter = new DataExporter();
            bool result = exporter.Export(names, out string errorMessage);

            // Assert: успех, но добавлена только одна валидная запись
            Assert.IsTrue(result);
            using var db = new AppDbContext();
            Assert.HasCount(1, db.People);
            Assert.IsTrue(db.People.Any(p => p.FullName == "Валидное Имя"));
        }

        /// <summary>
        /// Негативный тест 6: Импорт из пустой БД — возвращается сообщение о пустоте.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ImportFromEmptyDatabase_ReturnsEmptyMessage()
        {
            var importer = new DataImporter();
            importer.Import(out string line);

            // Assert
            Assert.AreEqual("База данных пуста", line);
        }

        /// <summary>
        /// Негативный тест 7: Импорт с фильтром, который ничего не находит.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ImportWithNonMatchingFilter_ReturnsEmptyList()
        {
            // Arrange
            using (var db = new AppDbContext())
            {
                db.People.Add(new Person { FullName = "Иван Иванов" });
                db.SaveChanges();
            }

            // Act
            var importer = new DataImporter();
            var result = importer.ImportByFilter("НесуществующееФамилия");

            // Assert
            Assert.HasCount(0, result);
        }

        /// <summary>
        /// Негативный тест 8: Импорт с null фильтром — возвращает все записи.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ImportWithNullFilter_ReturnsAll()
        {
            // Arrange
            using (var db = new AppDbContext())
            {
                db.People.Add(new Person { FullName = "Алексей Тестов" });
                db.People.Add(new Person { FullName = "Борис Тестов" });
                db.SaveChanges();
            }

            // Act
            var importer = new DataImporter();
            var result = importer.ImportByFilter(null!);

            // Assert
            Assert.HasCount(2, result);
        }

        /// <summary>
        /// Негативный тест 9: GetRecordCount на пустой БД — возвращает 0.
        /// </summary>
        [TestMethod]
        public void NegativeTest_RecordCountOnEmptyDatabase_ReturnsZero()
        {
            var exporter = new DataExporter();
            var importer = new DataImporter();

            Assert.AreEqual(0, exporter.GetRecordCount());
            Assert.AreEqual(0, importer.GetRecordCount());
        }

        /// <summary>
        /// Негативный тест 10: Import() не бросает исключение — возвращает пустой список.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ImportDoesNotThrow_ReturnsEmptyListOnError()
        {
            var importer = new DataImporter();
            List<Person>? result = null;

            try
            {
                result = importer.Import();
            }
            catch
            {
                Assert.Fail("Import() не должен выбрасывать исключения");
            }

            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Негативный тест 11: Import(out string) не выбрасывает исключение.
        /// </summary>
        [TestMethod]
        public void NegativeTest_ImportOutStringDoesNotThrow()
        {
            var importer = new DataImporter();
            string? result = null;

            try
            {
                importer.Import(out result);
            }
            catch
            {
                Assert.Fail("Import(out string) не должен выбрасывать исключения");
            }

            Assert.IsNotNull(result);
        }

        #endregion
    }
}
