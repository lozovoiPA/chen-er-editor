using ErEditor.ErSchemaClasses;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchema
{
    public abstract class DbErElement : IDbEntry
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public abstract class DbErElementWithAttributes : DbErElement
    {
        public virtual ObservableCollectionListSource<DbAttribute> Attributes { get; set; } = new();
    }

    public class DbEntitySet : DbErElementWithAttributes
    {
        public virtual ObservableCollectionListSource<DbRole> Roles { get; } = new();

        public DbEntitySet(string? name = null)
        {
            this.Name = name;
        }
    }

    public class DbRelationshipSet : DbErElementWithAttributes
    {
        public virtual ObservableCollectionListSource<DbRole> EntitySets { get; } = new();
        public virtual ObservableCollectionListSource<DbMapping> Mappings { get; } = new();

        public DbRelationshipSet(string? name = null)
        {
            this.Name = name;
        }
    }

    public class DbValueSet : DbErElement
    {
        public virtual ObservableCollectionListSource<DbAttribute> Attributes { get; } = new();

        public DbValueSet(string? name = null)
        {
            this.Name = name;
        }
    }
}
