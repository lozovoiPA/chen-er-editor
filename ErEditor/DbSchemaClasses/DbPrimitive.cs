using ErEditor.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErEditor.DbSchemaClasses
{
    [Table("Primitives")]
    public abstract class DbPrimitive : IDbEntry
    {
        [Key]
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string Type { get; set; } = null!;

        public int DiagramId { get; set; }
        public virtual DbDiagram Diagram { get; set; } = null!;

    }

    public abstract class DbShape : DbPrimitive
    {
        public int ElementWithAttributesId { get; set; }
    }

    public class DbRectangle : DbShape
    {
        public virtual DbEntitySet ElementWithAttributes { get; set; } = null!;
    }

    public class DbDiamond : DbShape
    {
        public virtual DbRelationshipSet ElementWithAttributes { get; set; } = null!;
    }

    public class DbEdge : DbPrimitive
    {
        public int RoleId { get; set; }
        public virtual DbRole Role { get; set; } = null!;
    }
}
