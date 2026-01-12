using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErEditor.DbSchema
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
        public int ElementId { get; set; }
    }

    public class DbRectangle : DbShape
    {
        public virtual DbEntitySet Element { get; set; } = null!;
    }

    public class DbRhombus : DbShape
    {
        public virtual DbRelationshipSet Element { get; set; } = null!;
    }

    public class DbAssociation : DbPrimitive
    {
        public virtual DbRole Role { get; set; } = null!;
    }
}
