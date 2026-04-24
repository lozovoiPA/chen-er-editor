using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
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
    public abstract class ErDiagramPrimitive : IObservable
    {
        public int X;
        public int Y;
        public int Z;

        public int width;
        public int height;

        protected string displayName;

        protected readonly ObservableBase observers = new();
        public bool BlockNotifying { get => observers.BlockNotifying; set => observers.BlockNotifying = value; }

        public abstract string Label { get; set; }
        
        public abstract ErElement ErElement { get; }


        public abstract void Draw(Graphics g);
        public abstract bool Intersects(Point point);

        public bool Subscribe(IObserver observer)
        {
            return observers.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observers.Unsubscribe(observer);
        }
    }

    public class ErDiagramEdge : ErDiagramPrimitive
    {
        public ErDiagramPrimitive pr1 = null!;
        public ErDiagramPrimitive pr2 = null!;
        public Point margin1;
        public Point margin2;

        private ErRole role;

        public ErDiagramEdge(ErRole role, ErDiagramPrimitive pr1, ErDiagramPrimitive pr2, Point margin1, Point margin2)
        {
            this.pr1 = pr1;
            this.pr2 = pr2;

            this.margin1 = margin1;
            this.margin2 = margin2;

            this.role = role;

            Z = -1;
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
            brush1.Dispose();
        }
        public override bool Intersects(Point point)
        {

            if ((X < point.X) && (X + width > point.X) && (Y < point.Y) && (Y + height > point.Y))
            {
                return true;
            }
            return false;
        }
    }

    public class ErDiagramRectangle : ErDiagramPrimitive
    {
        private ErEntitySet entitySet;

        public ErDiagramRectangle(ErEntitySet entitySet, int x, int y, int width = 100, int height = 30)
        {
            this.X = x;
            this.Y = y;

            this.width = width;
            this.height = height;

            this.entitySet = entitySet;

            Z = 0;
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
            SolidBrush outlineBrush = new SolidBrush(Color.Black);
            SolidBrush fillBrush = new SolidBrush(Color.White);

            g.DrawRectangle(new Pen(outlineBrush), X, Y, width, height);
            g.FillRectangle(fillBrush, X, Y, width, height);

            Font font = new Font(FontFamily.GenericSansSerif, 10);
            SizeF labelSize = g.MeasureString(this.Label, font);
            g.DrawString(this.Label, new Font(FontFamily.GenericSansSerif, 10), outlineBrush, X + width / 2 - labelSize.Width / 2 + 3, Y + height / 2 - labelSize.Height / 2);

            outlineBrush.Dispose();
            fillBrush.Dispose();
        }
        public override bool Intersects(Point point)
        {
            if ((X < point.X) && (X + width > point.X) && (Y < point.Y) && (Y + height > point.Y))
            {
                return true;
            }
            return false;
        }
    }
    public class ErDiagramDiamond : ErDiagramPrimitive
    {
        private ErRelationshipSet relationshipSet;

        public ErDiagramDiamond(ErRelationshipSet relationshipSet, int x, int y, int width = 100, int height = 30)
        {
            X = x;
            Y = y;

            base.width = width;
            base.height = height;

            this.relationshipSet = relationshipSet;

            Z = 0;
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
            SolidBrush outlineBrush = new SolidBrush(Color.Black);
            SolidBrush fillBrush = new SolidBrush(Color.White);

            Point[] points =
                [
                new Point(X + width / 2, Y),
                new Point(X + width - 2, Y + height / 2),
                new Point(X + width / 2, Y + height - 2),
                new Point(X, Y + height / 2),
                new Point(X + width / 2, Y)
                ];
            byte[] point_types = [(byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line];
            GraphicsPath line = new GraphicsPath(points, point_types);
            Region shape = new Region(line);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawLines(new Pen(outlineBrush, 2), points);
            g.FillRegion(fillBrush, shape);

            Font font = new Font(FontFamily.GenericSansSerif, 10);
            SizeF size = g.MeasureString(this.Label, font);
            if(size.Width >= 0.8 * width)
            {
                var split_strings = Label.Split(' ');
                double total_h = 0;
                double max_w = 0;
                foreach(var str in split_strings)
                {
                    var size_t = g.MeasureString(str, font);
                    total_h += size_t.Height + 1;
                    if(max_w < size_t.Width)
                    {
                        max_w = size_t.Width;
                    }
                }
                int text_y = (int)(Y + height / 2 - total_h / 2 + 1);

                foreach (var str in split_strings)
                {
                    var size_t = g.MeasureString(str, font);
                    g.FillRectangle(fillBrush, X + width / 2 - size_t.Width / 2 + 2, text_y + 2, size_t.Width - 2, size_t.Height - 2);
                    g.DrawString(str, font, outlineBrush, 
                        X + width / 2 - size_t.Width / 2 + 3, 
                        text_y);
                    text_y += 1 + (int)size_t.Height;
                }
                
            }
            else
            {
                g.DrawString(this.Label, font, outlineBrush, X + width / 2 - size.Width / 2 + 3, Y + height / 2 - size.Height / 2);
            }
            outlineBrush.Dispose();
            fillBrush.Dispose();
        }
        public override bool Intersects(Point point)
        {
            /*
            double parallelLine1 = ((double)(point.X - X - 5) / (width)) - ((double)(point.Y - Y + 5) / (height));
            double parallelLine2 = ((double)(point.X - X + 5) / (width)) - ((double)(point.Y - Y - 5) / (height));
            if (parallelLine1 * parallelLine2 < 0)
            {
                return true;
            }*/

            if ((X < point.X) && (X + width > point.X) && (Y < point.Y) && (Y + height > point.Y))
            {
                return true;
            }
            return false;
        }
    }


}
