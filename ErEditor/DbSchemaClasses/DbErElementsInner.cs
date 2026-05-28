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
    public class DbAttribute : DbErElement
    {
        public int ErElementWithAttributesId { get; set; }
        public virtual DbErElementWithAttributes ErElementWithAttributes { get; set; } = null!;

        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string? AllowedValues { get; set; }
        public bool IsKey { get; set; }

        public virtual ObservableCollectionListSource<DbValueSet> ValueSets { get; } = new();
        //public virtual ObservableCollectionListSource<DbAttributeDbValueSet> AttributeValueSets { get; } = new();

        public DbAttribute(string? name = null)
        {
            this.Name = name;
        }
    }

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

        public DbRole(string? name = null)
        {
            this.Name = name;
        }
    }

    public class DbMapping : DbErElement
    {
        public int RelationshipSetId { get; set; }
        public virtual DbRelationshipSet RelationshipSet { get; set; } = null!;
        public virtual ObservableCollectionListSource<DbMappingDbRole> MappingRoles { get; set; } = new();
        public int? MinCardinalityOfImage { get; set; }
        public int? MaxCardinalityOfImage { get; set; }
        public int? MinCardinalityOfPreImage { get; set; }
        public int? MaxCardinalityOfPreImage { get; set; }

        public DbMapping(string? name = null)
        {
            this.Name = name;
        }
    }
    
    [PrimaryKey(nameof(MappingId), nameof(RoleId))]
    [Table("MappingRole")]
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

    [PrimaryKey(nameof(AttributeId), nameof(ValueSetId))]
    [Table("AttributeValueSet")]
    public class DbAttributeDbValueSet
    {
        public int AttributeId { get; set; }
        //public virtual DbAttribute Attribute { get; set; } = null!;
        public int ValueSetId { get; set; }
        //public virtual DbValueSet ValueSet { get; set; } = null!;
    }
}
