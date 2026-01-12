using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public abstract class DiagramPrimitive
    {
        public int X;
        public int Y;

        public int width;
        public int height;

        public abstract string Label { get; set; }
        public abstract void Draw(Graphics g);
        public abstract string GetCustomType();

        public abstract ErElement ErElement { get; }
    }

    public abstract class DiagramShape : DiagramPrimitive
    {

    }
    public abstract class DiagramAssociation : DiagramPrimitive
    {
        public DiagramPrimitive pr1 = null!;
        public DiagramPrimitive pr2 = null!;
    }

    public class DiagramEdge : DiagramAssociation
    {
        public Point margin1;
        public Point margin2;

        private ErRole role;

        public DiagramEdge(DiagramPrimitive pr1, DiagramPrimitive pr2, Point margin1, Point margin2, ErRole role)
        {
            this.pr1 = pr1;
            this.pr2 = pr2;

            this.margin1 = margin1;
            this.margin2 = margin2;

            this.role = role;
        }

        public override string Label
        {
            get { return role.Name; }
            set { role.Name = value; }
        }
        public override ErRole ErElement
        {
            get { return role; }
        }

        public override void Draw(Graphics g)
        {
            SolidBrush brush1 = new SolidBrush(Color.Black);
            X = pr1.X + margin1.X;
            Y = pr1.Y + margin1.Y;
            width = pr2.X + margin2.X;
            height = pr2.Y + margin2.Y;
            g.DrawLine(new Pen(brush1), X, Y, width, height);
            width -= X;
            height -= Y;
            //width = Math.Abs(width);
            //height = Math.Abs(height);

            brush1.Dispose();
        }
        public override string GetCustomType()
        {
            return "edge";
        }
    }

    public class DiagramRectangle : DiagramShape
    {
        private ErEntitySet entitySet;

        public DiagramRectangle(ErEntitySet entitySet, int x, int y, int width = 100, int height = 30)
        {
            this.X = x;
            this.Y = y;

            this.width = width;
            this.height = height;

            this.entitySet = entitySet;
        }

        public override string Label
        {
            get { return entitySet.Name; }
            set { entitySet.Name = value; }
        }
        public override ErEntitySet ErElement
        {
            get { return entitySet; }
        }

        public override void Draw(Graphics g)
        {
            SolidBrush brush1 = new SolidBrush(Color.Black);
            SolidBrush brush = new SolidBrush(Color.White);
            g.DrawRectangle(new Pen(brush1), X, Y, width, height);
            g.FillRectangle(brush, X + 1, Y + 1, width - 2, height - 2);

            Font font = new Font(FontFamily.GenericSansSerif, 10);
            SizeF size = g.MeasureString(this.Label, font);
            g.DrawString(this.Label, new Font(FontFamily.GenericSansSerif, 10), brush1, X + width / 2 - size.Width / 2, Y + height / 2 - size.Height / 2);

            brush1.Dispose();
            brush.Dispose();
        }
        public override string GetCustomType()
        {
            return "rect";
        }
    }
    public class DiagramRhombus : DiagramShape
    {
        private ErRelationshipSet relationshipSet;

        public DiagramRhombus(int _x, int _y, ErRelationshipSet relationshipSet, int _width = 100, int _height = 50)
        {
            X = _x;
            Y = _y;

            width = _width;
            height = _height;

            this.relationshipSet = relationshipSet; 
        }

        public override string Label
        {
            get { return relationshipSet.Name; }
            set { relationshipSet.Name = value; }
        }
        public override ErRelationshipSet ErElement
        {
            get { return relationshipSet; }
        }

        public override void Draw(Graphics g)
        {
            SolidBrush brush1 = new SolidBrush(Color.Black);
            SolidBrush brush = new SolidBrush(Color.White);

            Point[] points =
                [
                new Point(X + width / 2, Y + 1),
                new Point(X + width - 2, Y + height / 2),
                new Point(X + width / 2, Y + height - 2),
                new Point(X + 1, Y + height / 2),
                new Point(X + width / 2, Y + 1)
                ];
            byte[] point_types = [(byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line];
            GraphicsPath path = new GraphicsPath(points, point_types);
            Region region = new Region(path);

            g.DrawLines(new Pen(brush1, 2), points);
            g.FillRegion(brush, region);

            Font font = new Font(FontFamily.GenericSansSerif, 10);
            SizeF size = g.MeasureString(this.Label, font);
            g.DrawString(this.Label, font, brush1, X + width / 2 - size.Width / 2, Y + height / 2 - size.Height / 2);


            brush1.Dispose();
            brush.Dispose();
        }
        public override string GetCustomType()
        {
            return "rhombus";
        }
    }


}
