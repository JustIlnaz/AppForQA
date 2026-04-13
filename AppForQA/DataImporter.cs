using AppForQA.Context;
using AppForQA.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForQA
{
    public class DataImporter
    {
        /// <summary>
        /// Импортирует (считывает) все записи людей из базы данных.
        /// При ошибке возвращает пустой список.
        /// </summary>
        public List<Person> Import()
        {
            try
            {
                using var db = new AppDbContext();
                return db.People.ToList();
            }
            catch
            {
                return new List<Person>();
            }
        }

        /// <summary>
        /// Импортирует записи из БД и возвращает их в виде форматированной строки.
        /// При ошибке возвращает сообщение об ошибке.
        /// </summary>
        public void Import(out string line)
        {
            try
            {
                using var db = new AppDbContext();
                var people = db.People.ToList();

                if (people.Count == 0)
                {
                    line = "База данных пуста";
                    return;
                }

                var formatted = people.Select(p => $"{p.Id}. {p.FullName}");
                line = string.Join("\r\n", formatted);
            }
            catch (Exception ex)
            {
                line = $"Ошибка импорта: {ex.Message}";
            }
        }

        /// <summary>
        /// Импортирует записи, отфильтрованные по частичному совпадению имени.
        /// При ошибке возвращает пустой список.
        /// </summary>
        public List<Person> ImportByFilter(string searchText)
        {
            try
            {
                using var db = new AppDbContext();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return db.People.ToList();
                }

                return db.People
                    .Where(p => p.FullName != null && p.FullName.Contains(searchText))
                    .ToList();
            }
            catch
            {
                return new List<Person>();
            }
        }

        /// <summary>
        /// Возвращает количество записей в БД.
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
