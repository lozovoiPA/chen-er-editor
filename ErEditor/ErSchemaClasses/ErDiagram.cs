using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public class ErDiagram : IObservable, INamedObject, IEnumerable<ErDiagramPrimitive>
    {
        private string name = string.Empty;
        private List<ErDiagramPrimitive> primitives = new();

        private readonly ObservableBase observers = new();
        public bool BlockNotifying { get => observers.BlockNotifying; set => observers.BlockNotifying = value; }

        public ErDiagram() { }
        public ErDiagram(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
            set { name = value; observers.Notify(new ObjectNameChangedNotification(this, name)); }
        }

        public ErDiagramRectangle AddRectangle(ErEntitySet entitySet, int x, int y, int w = 100, int h = 30)
        {
            ErDiagramRectangle primitive = new ErDiagramRectangle(entitySet, x, y, w, h);
            primitives.Add(primitive);

            observers.Notify(new ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>(this, primitive));
            return primitive;
        }
        public ErDiagramDiamond AddDiamond(ErRelationshipSet relationshipSet, int x, int y, int w = 100, int h = 30)
        {
            ErDiagramDiamond primitive = new ErDiagramDiamond(relationshipSet, x, y, w, h);
            primitives.Add(primitive);

            observers.Notify(new ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>(this, primitive));
            return primitive;
        }
        public ErDiagramEdge AddEdge(ErRole role, ErDiagramPrimitive pr1, ErDiagramPrimitive pr2, Point p1, Point p2)
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
            ErDiagramEdge edge = new ErDiagramEdge(role, pr1, pr2, p1, p2);
            primitives.Add(edge);

            observers.Notify(new ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>(this, edge));
            return edge;
        }
        public ErDiagramEdge? AddEdge(ErRole role, ErRelationshipSet relationshipSet, Point p1, Point p2)
        {
            ErDiagramPrimitive? rectangle = primitives.Find(x => x.ErElement == role.EntitySet);
            ErDiagramPrimitive? diamond = primitives.Find(x => x.ErElement == relationshipSet);

            if(rectangle != null && diamond != null)
            {
                return this.AddEdge(role, rectangle, diamond, p1, p2);
            }
            return null;
        }

        public IEnumerator<ErDiagramPrimitive> GetEnumerator()
        {
            return primitives.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Draw(Graphics g)
        {
            foreach (var primitive in primitives)
            {
                primitive.Draw(g);
            }
        }
        public ErDiagramPrimitive? FindAt(int x, int y)
        {
            Point point = new(x, y);
            foreach (var primitive in primitives)
            {
                if (primitive.Intersects(point))
                {
                    return primitive;
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

        public override string ToString()
        {
            return this.name;
        }

        public bool Subscribe(IObserver observer)
        {
            return observers.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observers.Unsubscribe(observer);
        }
    }
}
