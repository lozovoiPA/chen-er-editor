using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ExportClasses
{
    public class ExportDbContext : DbContext
    {
        private readonly string dbFullPath = "";

        public ExportDbContext(string dbFullPath)
        {
            this.dbFullPath = dbFullPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=" + dbFullPath)
                .EnableSensitiveDataLogging();
        }
    }
}
