using AppForQA.Context;
using AppForQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForQA
{
    public class DataExporter
    {
        /// <summary>
        /// Экспортирует (добавляет) список имён в базу данных.
        /// Возвращает true при успехе, false при ошибке.
        /// </summary>
        public bool Export(IEnumerable<string> names, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (names == null)
                {
                    errorMessage = "Список имён не может быть null.";
                    return false;
                }

                using var db = new AppDbContext();

                foreach (var name in names)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue; // Пропускаем пустые имена
                        }

                        if (!db.People.Any(p => p.FullName == name))
                        {
                            db.People.Add(new Person { FullName = name });
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Ошибка при добавлении '{name}': {ex.Message}";
                        return false;
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ошибка экспорта: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Экспортирует одно имя в базу данных.
        /// Возвращает true при успехе, false при ошибке.
        /// </summary>
        public bool Export(string fullName, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (fullName == null)
                {
                    errorMessage = "Имя не может быть null.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    errorMessage = "Имя не может быть пустым или состоять из пробелов.";
                    return false;
                }

                using var db = new AppDbContext();

                if (!db.People.Any(p => p.FullName == fullName))
                {
                    db.People.Add(new Person { FullName = fullName });
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ошибка экспорта: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Возвращает общее количество записей в БД.
        /// При ошибке возвращает -1.
        /// </summary>
        public int GetRecordCount()
        {
            try
            {
                using var db = new AppDbContext();
                return db.People.Count();
            }
            catch
            {
                return -1;
            }
        }
    }
}
