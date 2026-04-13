using AppForQA.Context;
using System.IO;
using System.Linq;

namespace AppForQA
{
    public class DataExporter
    {
        public void Export(string path)
        {
            using var db = new AppDbContext();
            var people = db.People.ToList();

            using var writer = new StreamWriter(path);
            foreach (var person in people)
            {
                writer.WriteLine($"{person.Id};{person.FullName}");
            }
            writer.Flush();
        }
    }
}
