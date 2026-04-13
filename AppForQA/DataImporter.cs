using AppForQA.Context;
using AppForQA.Models;
using System;
using System.IO;
using System.Linq;

namespace AppForQA
{
    public class DataImporter
    {
        public void Import(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Файл '{path}' не найден.");
            }

            using var db = new AppDbContext();
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                var parts = line.Split(';');
                string fullName;
                if (parts.Length >= 2)
                {
                    // Формат "Id;FullName"
                    fullName = parts[1];
                }
                else
                {
                    // Формат "FullName" (простая строка)
                    fullName = parts[0];
                }

                if (!db.People.Any(p => p.FullName == fullName))
                {
                    db.People.Add(new Person { FullName = fullName });
                }
            }

            db.SaveChanges();
        }

        public void Import(string path, out string line)
        {
            if (!File.Exists(path))
            {
                line = string.Empty;
                throw new FileNotFoundException($"Файл '{path}' не найден.");
            }

            line = File.ReadAllText(path);

            // Также импортируем данные в БД
            var lines = line.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            using var db = new AppDbContext();

            foreach (var l in lines)
            {
                var parts = l.Split(';');
                string fullName;
                if (parts.Length >= 2)
                {
                    fullName = parts[1];
                }
                else
                {
                    fullName = parts[0];
                }

                if (!db.People.Any(p => p.FullName == fullName))
                {
                    db.People.Add(new Person { FullName = fullName });
                }
            }

            db.SaveChanges();
        }
    }
}
