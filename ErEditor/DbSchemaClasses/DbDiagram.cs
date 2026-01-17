using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{
    public class DbDiagram : IDbEntry
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public virtual ObservableCollectionListSource<DbPrimitive> Primitives { get; set; } = new();

        public DbDiagram(string? name = null)
        {
            this.Name = name;
        }
    }
}
