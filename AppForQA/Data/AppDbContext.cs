using AppForQA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AppForQA.Context
{
    public class AppDbContext : DbContext
{
    public DbSet<Person> People { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql("Host=localhost;Port=5432;Database=dbForQA;Username=postgres;Password=love");
    }
}
