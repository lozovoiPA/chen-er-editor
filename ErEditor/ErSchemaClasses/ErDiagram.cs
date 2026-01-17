using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public interface IErDiagram : IErElement { }
    public class ErDiagram : IErDiagram
    {
        private string name = String.Empty;
        private readonly ErSchema schema;

        //private List<DiagramShape> shapes        = new();
        private List<DiagramRectangle> rectangles   = new();
        private List<DiagramRhombus> rhombuses      = new();
        private List<DiagramAssociation> edges      = new();

        private readonly ObservableBase observableLogic = new();

        public ErDiagram(ErSchema schema, string name = "")
        {
            this.schema = schema;
            this.name = name;
        }

        public string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification(this, name)); }
        }

        public ErSchema Schema
        {
            get { return schema; }
        }

        private DiagramRectangle AddRectangle_Inner(ErEntitySet entitySet, int x, int y, int w, int h)
        {
            DiagramRectangle pr = new DiagramRectangle(entitySet, x, y, w, h);
            rectangles.Add(pr);
            return pr;
        }
        public DiagramRectangle AddRectangle(int x, int y, int w = 100, int h = 30)
        {
            ErEntitySet el = schema.AddEntitySet();
            var pr = AddRectangle_Inner(el, x, y, w, h);
            return pr;
        }
        public DiagramRectangle? AddRectangle(ErEntitySet entitySet, int x, int y, int w = 100, int h = 30)
        {
            if (!schema.FindEntitySet(entitySet))
            {
                ConsoleLog.Log($"Cannot find entity set {entitySet.Name} on parent schema of diagram {this.name}." +
                    $"A rectangle for this entity set will not be added.", this, "WARNING");
                return null;
            }
            var pr = AddRectangle_Inner(entitySet, x, y, w, h);
            return pr;
        }
        public DiagramRhombus AddRhombus(int x, int y, int w = 100, int h = 30)
        {
            ErRelationshipSet el = schema.AddRelationshipSet();
            DiagramRhombus pr = new DiagramRhombus(x, y, el, w, h);
            rhombuses.Add(pr);
            return pr;
        }

        /*
        public DiagramAssociation AddEdge(DiagramPrimitive pr1, DiagramPrimitive pr2, Point p1, Point p2)
        {
            if (p1.Y > p2.Y) // because we are assigning p1 - top left, p2 - bottom right, but this link (line) may be upside down (pr1 is always above pr2)
            {
                var t = p1;
                p1 = p2;
                p2 = t;

                var t1 = pr1;
                pr1 = pr2;
                pr2 = t1;
            }
            p1.X -= pr1.X;
            p1.Y -= pr1.Y;
            p2.X -= pr2.X;
            p2.Y -= pr2.Y;
            DiagramEdge edge = new DiagramEdge(pr1, pr2, p1, p2);
            edges.Add(edge);

            return edge;
        }*/

        public void Draw(Graphics g)
        {
            foreach (DiagramAssociation lk in edges)
            {
                lk.Draw(g);
            }
            foreach (var pr in rectangles)
            {
                pr.Draw(g);
            }
            foreach (var pr in rhombuses)
            {
                pr.Draw(g);
            }
        }
        public DiagramPrimitive? FindAt(int x, int y)
        {
            foreach (DiagramShape pr in rectangles)
            {
                if ((pr.X < x) && (pr.X + pr.width > x) && (pr.Y < y) && (pr.Y + pr.height > y))
                {
                    return pr;
                }
            }
            foreach (DiagramShape pr in rhombuses)
            {
                if ((pr.X < x) && (pr.X + pr.width > x) && (pr.Y < y) && (pr.Y + pr.height > y))
                {
                    return pr;
                }
            }
            foreach (DiagramAssociation lk in edges)
            {
                double calc1 = ((double)(x - lk.X - 5) / (lk.width)) - ((double)(y - lk.Y + 5) / (lk.height));
                double calc2 = ((double)(x - lk.X + 5) / (lk.width)) - ((double)(y - lk.Y - 5) / (lk.height));
                if (calc1 * calc2 < 0)
                {
                    return lk;
                }
            }
            return null;
        }

        /*
        public void Remove(DiagramPrimitive _pr)
        {
            DiagramPrimitive? pr = shapes.Find(x => x == _pr);
            if (pr != null)
            {
                shapes.Remove((DiagramShape)_pr);
                foreach (var item in edges.FindAll(x => (x.pr1 == _pr) || (x.pr2 == _pr)))
                {
                    edges.Remove(item);
                }
            }
            else
            {
                edges.Remove((DiagramAssociation)_pr);
            }
        }
        */

        public bool Subscribe(IObserver observer)
        {
            return observableLogic.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observableLogic.Unsubscribe(observer);
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
