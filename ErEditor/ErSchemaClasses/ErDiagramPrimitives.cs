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
        private int x;
        private int y;
        private int width;
        private int height;
        

        public int X
        {
            get { return x; }
            set { x = value; observers.Notify(new ObjectUpdatedNotification<ErDiagramPrimitive>(this)); }
        }

        public int Y
        {
            get { return y; }
            set { y = value; observers.Notify(new ObjectUpdatedNotification<ErDiagramPrimitive>(this)); }
        }
        public int Z;

        public int Width
        {
            get { return width; }
            set { width = value; observers.Notify(new ObjectUpdatedNotification<ErDiagramPrimitive>(this)); }
        }
        public int Height
        {
            get { return height; }
            set { height = value; observers.Notify(new ObjectUpdatedNotification<ErDiagramPrimitive>(this)); }
        }

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

        protected virtual void DrawSplitLabel(Font font, Graphics g, SolidBrush fillBrush, SolidBrush outlineBrush)
        {
            SizeF size = g.MeasureString(this.Label, font);
            if (size.Width >= 0.8 * Width)
            {
                var split_strings = Label.Split(' ');
                double total_h = 0;
                double max_w = 0;
                foreach (var str in split_strings)
                {
                    var size_t = g.MeasureString(str, font);
                    total_h += size_t.Height + 1;
                    if (max_w < size_t.Width)
                    {
                        max_w = size_t.Width;
                    }
                }
                int text_y = (int)(Y + Height / 2 - total_h / 2 + 1);

                foreach (var str in split_strings)
                {
                    var size_t = g.MeasureString(str, font);
                    g.FillRectangle(fillBrush, X + Width / 2 - size_t.Width / 2 + 2, text_y + 2, size_t.Width - 2, size_t.Height - 2);
                    g.DrawString(str, font, outlineBrush,
                        X + Width / 2 - size_t.Width / 2 + 3,
                        text_y);
                    text_y += 1 + (int)size_t.Height;
                }

            }
            else
            {
                var size_t = g.MeasureString(Label, font);
                g.FillRectangle(fillBrush, X + Width / 2 - size_t.Width / 2 + 2, Y + Height / 2 - size.Height / 2, size_t.Width - 2, size_t.Height - 2);
                g.DrawString(this.Label, font, outlineBrush, X + Width / 2 - size.Width / 2 + 3, Y + Height / 2 - size.Height / 2);
            }
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

            var start = GetStartPoint();
            var end = GetEndPoint();
            this.X = start.X;
            this.Y = start.Y;
            this.Width = end.X - start.X;
            this.Height = end.Y - start.Y;

            this.role = role;

            Console.WriteLine($"Edge for {role} finished constructor with {X}, {Y} and {Width}, {Height}");

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

        private void DrawLabel(Graphics g, Point start, Point end)
        {
            if (!string.IsNullOrEmpty(Label))
            {
                Point midPoint = new Point(
                    (start.X + end.X) / 2,
                    (start.Y + end.Y) / 2
                );

                /*
                using (Font font = new Font("Arial", 8))
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    SizeF textSize = g.MeasureString(Label, font);
                    g.DrawString(Label, font, brush,
                        midPoint.X - textSize.Width / 2,
                        midPoint.Y - textSize.Height / 2);
                }*/
                Font font = new Font("Arial", 8);
                SolidBrush brush = new SolidBrush(Color.White);
                SolidBrush brush2 = new SolidBrush(Color.Black);
                DrawSplitLabel(font, g, brush, brush2);
            }
        }

        private Point GetStartPoint()
        {
            return new Point(pr1.X + margin1.X, pr1.Y + margin1.Y);
        }

        private Point GetEndPoint()
        {
            return new Point(pr2.X + margin2.X, pr2.Y + margin2.Y);
        }

        public override bool Intersects(Point point)
        {
            Point start = GetStartPoint();
            Point end = GetEndPoint();

            return PointToLineDistance(point, start, end) <= 5;
        }

        private float PointToLineDistance(Point point, Point lineStart, Point lineEnd)
        {
            float dx = lineEnd.X - lineStart.X;
            float dy = lineEnd.Y - lineStart.Y;
            float lengthSquared = dx * dx + dy * dy;
            if (lengthSquared == 0)
            {
                float distToPoint = (float)Math.Sqrt(
                    Math.Pow(point.X - lineStart.X, 2) +
                    Math.Pow(point.Y - lineStart.Y, 2)
                );
                return distToPoint;
            }

            float t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            float closestX = lineStart.X + t * dx;
            float closestY = lineStart.Y + t * dy;

            float distance = (float)Math.Sqrt(
                Math.Pow(point.X - closestX, 2) +
                Math.Pow(point.Y - closestY, 2)
            );

            return distance;
        }

        public override void Draw(Graphics g)
        {
            Point startPoint = GetStartPoint();
            Point endPoint = GetEndPoint();
            X = startPoint.X;
            Y = startPoint.Y;
            this.Width = endPoint.X - startPoint.X;
            this.Height = endPoint.Y - startPoint.Y;

            //var X_t = Math.Min(startPoint.X, endPoint.X);
            //var Y_t = Math.Min(startPoint.Y, endPoint.Y);
            //var width_t = Math.Abs(startPoint.X - endPoint.X);
            //var height_t = Math.Abs(startPoint.Y - endPoint.Y);

            using (Pen pen = new Pen(Color.Black))
            {
                g.DrawLine(pen, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
            }
            DrawLabel(g, startPoint, endPoint);
        }
    }

    public class ErDiagramRectangle : ErDiagramPrimitive
    {
        private ErEntitySet entitySet;

        public ErDiagramRectangle(ErEntitySet entitySet, int x, int y, int width = 100, int height = 30)
        {
            this.X = x;
            this.Y = y;

            this.Width = width;
            this.Height = height;

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

            g.DrawRectangle(new Pen(outlineBrush), X, Y, Width, Height);
            g.FillRectangle(fillBrush, X, Y, Width, Height);

            Font font = new Font(FontFamily.GenericSansSerif, 10);
            SizeF labelSize = g.MeasureString(this.Label, font);
            g.DrawString(this.Label, new Font(FontFamily.GenericSansSerif, 10), outlineBrush, X + Width / 2 - labelSize.Width / 2 + 3, Y + Height / 2 - labelSize.Height / 2);

            outlineBrush.Dispose();
            fillBrush.Dispose();
        }
        public override bool Intersects(Point point)
        {
            if ((X < point.X) && (X + Width > point.X) && (Y < point.Y) && (Y + Height > point.Y))
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

            base.Width = width;
            base.Height = height;

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
                new Point(X + Width / 2, Y),
                new Point(X + Width - 2, Y + Height / 2),
                new Point(X + Width / 2, Y + Height - 2),
                new Point(X, Y + Height / 2),
                new Point(X + Width / 2, Y)
                ];
            byte[] point_types = [(byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line];
            GraphicsPath line = new GraphicsPath(points, point_types);
            Region shape = new Region(line);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawLines(new Pen(outlineBrush, 2), points);
            g.FillRegion(fillBrush, shape);

            Font font = new Font(FontFamily.GenericSansSerif, 10);
            DrawSplitLabel(font, g, fillBrush, outlineBrush);
            
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

            if ((X < point.X) && (X + Width > point.X) && (Y < point.Y) && (Y + Height > point.Y))
            {
                return true;
            }
            return false;
        }
    }


}
