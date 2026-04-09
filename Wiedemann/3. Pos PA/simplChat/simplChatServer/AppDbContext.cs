using simplChatShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace simplChatServer
{
    //Nuget Pakete:
    //Microsoft.EntityFrameworkCore
    //Microsoft.EntityFrameworkCore.SQLite
    //Microsoft.EntityFrameworkCore.Tools
    //using Microsoft.EntityFrameworkCore;
    //im constructor db.Database.EnsureCreated();
    //und die db datei copy if newer machen
    public class AppDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=ChatDatenbank.db");
    }
}
