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

        public int ElementId { get; set; }
        public virtual DbErElement Element { get; set; } = null!;

    }

    public abstract class DbShape : DbPrimitive
    {

    }

    public class DbRectangle : DbShape
    {
        public new virtual DbEntitySet Element { get; set; } = null!;
    }

    public class DbDiamond : DbShape
    {
        public new virtual DbRelationshipSet Element { get; set; } = null!;
    }

    public class DbEdge : DbPrimitive
    {
        public new virtual DbRole Element { get; set; } = null!;
    }
}
