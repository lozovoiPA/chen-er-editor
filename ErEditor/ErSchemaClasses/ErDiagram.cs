using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Media3D;

namespace ErEditor.ErSchemaClasses
{
    public class ErDiagram : IObservable, INamedObject, IEnumerable<ErDiagramPrimitive>, 
        IObserver,
        IVisitor<ObjectDeletedNotification<ErEntitySet>>,
        IVisitor<ObjectDeletedNotification<ErRelationshipSet>>,
        IVisitor<ObjectDeletedNotification<ErRole>>
    {
        public ObserverBase observerLogic;

        private string name = string.Empty;
        public List<ErDiagramPrimitive> primitives = new();

        private readonly ObservableBase observers = new();
        public bool BlockNotifying { get => observers.BlockNotifying; set => observers.BlockNotifying = value; }

        public ErDiagram() { observerLogic = new(this); }
        public ErDiagram(string name)
        {
            this.name = name;
            observerLogic = new(this);
        }

        public string Name
        {
            get { return name; }
            set { name = value; observers.Notify(new ObjectNameChangedNotification(this, name)); }
        }

        public void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
        public void Visit(ObjectDeletedNotification<ErEntitySet> notif)
        {
            var prims = primitives.Where(x => x.ErElement == notif.Object).ToList();
            foreach (var pr in prims)
            {
                Remove(pr);
            }
        }
        public void Visit(ObjectDeletedNotification<ErRelationshipSet> notif)
        {
            var prims = primitives.Where(x => x.ErElement == notif.Object).ToList();
            foreach (var pr in prims)
            {
                Remove(pr);
            }
        }
        public void Visit(ObjectDeletedNotification<ErRole> notif)
        {
            var prims = primitives.Where(x => x.ErElement == notif.Object).ToList();
            foreach (var pr in prims)
            {
                Remove(pr);
            }
        }

        public Rectangle GetSize()
        {
            int minx = int.MaxValue, miny = int.MaxValue, maxx = 0, maxy = 0;
            var shapes = primitives.Where(x => x is ErDiagramDiamond || x is ErDiagramRectangle).ToList();
            foreach(var pr in shapes)
            {
                if(pr.X < minx) minx = pr.X;
                if (pr.Y < miny) miny = pr.Y;
                if((pr.X + pr.width) > maxx) maxx = pr.X + pr.width;
                if ((pr.Y + pr.height) > maxy) maxy = pr.Y + pr.height;

                Console.WriteLine($"Primitive {pr.Label}: {pr.X}, {pr.Y}, {pr.X + pr.width}, {pr.Y + pr.height}");
            }
            Console.WriteLine($"{maxx}, {maxy}");
            return new Rectangle(0, 0, maxx - 0 + (minx), maxy - 0 + (miny));
        }

        public ErDiagramRectangle AddRectangle(ErEntitySet entitySet, int x, int y, int w = 100, int h = 30)
        {
            ErDiagramRectangle primitive = new ErDiagramRectangle(entitySet, x, y, w, h);
            primitives.Add(primitive);

            observers.Notify(new ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>(this, primitive));
            return primitive;
        }
        public ErDiagramDiamond AddDiamond(ErRelationshipSet relationshipSet, int x, int y, int w = 120, int h = 40)
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

            if(pr1 is ErDiagramDiamond && pr2 is ErDiagramRectangle)
            {
                var p3 = p2;
                var pr3 = pr2;
                p2 = p1;
                pr2 = pr1;

                p1 = p3;
                pr1 = pr3;
            }

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
            g.SmoothingMode = SmoothingMode.HighQuality;
            var drawpr = primitives.OrderBy(x => x.Z).ToList();
            foreach (var primitive in drawpr)
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

        public void Remove(ErDiagramPrimitive _pr)
        {
            ErDiagramPrimitive? pr = primitives.Find(x => x == _pr);
            if (pr != null)
            {
                primitives.Remove(_pr);
                observers.Notify(new ObjectDeletedNotification<ErDiagramPrimitive>(_pr));
                if(_pr is ErDiagramRectangle || _pr is ErDiagramDiamond)
                {
                    var edges = primitives.FindAll(x => x is ErDiagramEdge).ConvertAll<ErDiagramEdge>(x=> x as ErDiagramEdge);
                    foreach (var item in edges.FindAll(x => (x.pr1 == _pr) || (x.pr2 == _pr)))
                    {
                        primitives.Remove(item);
                        observers.Notify(new ObjectDeletedNotification<ErDiagramPrimitive>(item));
                    }
                }
                
            }
        }

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
