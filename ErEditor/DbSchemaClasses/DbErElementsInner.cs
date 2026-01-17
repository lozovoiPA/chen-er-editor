using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{
    [Table("Attributes")]
    public class DbAttribute : DbErElement
    {
        public int ErElementWithAttributesId { get; set; }
        public virtual DbErElementWithAttributes ErElementWithAttributes { get; set; } = null!;

        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string? AllowedValues { get; set; }
        public bool IsKey { get; set; }

        public virtual ObservableCollectionListSource<DbValueSet> ValueSets { get; set; } = new();

        [NotMapped]
        public EntityState State { get; set; }

        public DbAttribute(string? name = null)
        {
            this.Name = name;
        }
    }

    [Table("Roles")]
    public class DbRole : DbErElement
    {
        //[Key, Column(Order = 0)]
        public int EntitySetId { get; set; }
        public virtual DbEntitySet EntitySet { get; set; } = null!;
        //[Key, Column(Order = 0)]
        public int RelationshipSetId { get; set; }
        public virtual DbRelationshipSet RelationshipSet { get; set; } = null!;
        public bool IsKeyEntitySet { get; set; }
        public bool IsIdDependant { get; set; }
        public virtual ObservableCollectionListSource<DbMappingDbRole> MappingDbRoles { get; } = new();

        public DbRole(string? name = null)
        {
            this.Name = name;
        }
    }

    [Table("Mappings")]
    public class DbMapping : DbErElement
    {
        public int RelationshipSetId { get; set; }
        public virtual DbRelationshipSet RelationshipSet { get; set; } = null!;

        public int? MinCardinalityOfImage { get; set; }
        public int? MaxCardinalityOfImage { get; set; }
        public int? MinCardinalityOfPreimage { get; set; }
        public int? MaxCardinalityOfPreimage { get; set; }

        public DbMapping(string? name = null)
        {
            this.Name = name;
        }
    }

    [PrimaryKey(nameof(MappingId), nameof(RoleId))]
    public class DbMappingDbRole
    {
        //[Key, Column(Order = 0)]
        public int MappingId { get; set; }
        public virtual DbMapping Mapping { get; set; } = null!;
        //[Key, Column(Order = 1)]
        public int RoleId { get; set; }
        public virtual DbRole Role { get; set; } = null!;
        public string? Type { get; set; }
    }


    // можно вручную сюда добавить доп сущность на таблицу если бабанову захочется только 1 таблицу M:M
}
