using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public partial class ErSchema : 
        IObservable, IObserver
    {
        private string name = String.Empty;

        private List<ErEntitySet> entitySets             = new(); // без всяких observablecollection - просто метод AddEntitySet привязываем куда это надо (к ноду, к графическому элементу и т.д.)
        private List<ErRelationshipSet> relationshipSets = new();
        private List<ErValueSet> valueSets               = new();
        private List<ErDiagram> diagrams                 = new();

        private readonly ObservableBase observableLogic = new();
        private Visitor visitorLogic;

        public ErSchema(string name = "") 
        {
            this.name = name;
            visitorLogic = new(this);
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public List<ErEntitySet> EntitySets
        {
            get { return entitySets; }
        }
        public List<ErRelationshipSet> RelationshipSets
        {
            get { return relationshipSets; }
        }
        public List<ErValueSet> ValueSets
        {
            get { return valueSets; }
        }
        public List<ErDiagram> Diagrams
        {
            get { return diagrams; }
        }

        public bool FindEntitySet(ErEntitySet entitySet)
        {
            return !(entitySets.Find(x => x == entitySet) is null);
        }
        public bool FindRelationshipSet(ErRelationshipSet relationshipSet)
        {
            return !(relationshipSets.Find(x => x == relationshipSet) is null);
        }
        public ErEntitySet? FindEntitySet(string name)
        {
            return entitySets.Find(x => x.Name == name);
        }
        public ErValueSet? FindValueSet(string name)
        {
            return valueSets.Find(x => x.Name == name);
        }

        public ErEntitySet AddEntitySet(string name = "")
        {
            ErEntitySet es = new(name);
            entitySets.Add(es);

            es.Subscribe(this);

            observableLogic.Notify(new ObjectAddedNotification<ErEntitySet>(es));
            return es;
        }
        public void AddEntitySetRange(List<ErEntitySet> range)
        {
            entitySets.AddRange(range);

            // Пока немножко не оптимизированно, но ничего страшного
            // Все получится. Все получится!
            foreach(var es in range)
            {
                es.Subscribe(this);
                observableLogic.Notify(new ObjectAddedNotification<ErEntitySet>(es));
            }
        }
        public ErRelationshipSet AddRelationshipSet(string name = "")
        {
            ErRelationshipSet rs = new(name);
            relationshipSets.Add(rs);

            rs.Subscribe(this);

            observableLogic.Notify(new ObjectAddedNotification<ErRelationshipSet>(rs));
            return rs;
        }
        public void AddRelationshipSetRange(List<ErRelationshipSet> range)
        {
            relationshipSets.AddRange(range);

            foreach (var rs in range)
            {
                rs.Subscribe(this);
                observableLogic.Notify(new ObjectAddedNotification<ErRelationshipSet>(rs));
            }
        }
        public ErValueSet AddValueSet(string name = "")
        {
            ErValueSet vs = new(name);
            valueSets.Add(vs);

            vs.Subscribe(this);

            observableLogic.Notify(new ObjectAddedNotification<ErValueSet>(vs));
            return vs;
        }
        public void AddValueSetRange(List<ErValueSet> range)
        {
            valueSets.AddRange(range);

            foreach (var vs in range)
            {
                vs.Subscribe(this);
                observableLogic.Notify(new ObjectAddedNotification<ErValueSet>(vs));
            }
        }
        public ErDiagram AddDiagram(string name = "")
        {
            ErDiagram dgr = new(this, name);
            diagrams.Add(dgr);

            dgr.Subscribe(this);

            observableLogic.Notify(new ObjectAddedNotification<ErDiagram>(dgr));
            return dgr;
        }
        public void AddDiagramRange(List<ErDiagram> range)
        {
            diagrams.AddRange(range);

            foreach (var dgr in range)
            {
                dgr.Subscribe(this);
                observableLogic.Notify(new ObjectAddedNotification<ErDiagram>(dgr));
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
        public string PrintState()
        {
            string output = "";
            output += $"Множества сущностей (всего - {entitySets.Count}):\n";
            foreach (var el in entitySets)
            {
                output += $"\t{el.Name}\n";
            }
            output += $"Множества связей (всего - {relationshipSets.Count}):\n";
            foreach (var el in relationshipSets)
            {
                output += $"\t{el.Name}\n";
            }
            output += $"Множества значений (всего - {valueSets.Count}):\n";
            foreach (var el in valueSets)
            {
                output += $"\t{el.Name}\n";
            }
            output += $"Диаграммы (всего - {diagrams.Count}):\n";
            foreach (var el in diagrams)
            {
                output += $"\t{el.Name}\n";
            }
            return output;
        }

        public bool Subscribe(IObserver observer)
        {
            return observableLogic.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observableLogic.Unsubscribe(observer);
        }

        public void Recieve(Notification notification)
        {
            notification.Accept(visitorLogic);
        }
    }
}
