using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public partial class ErSchema : 
        IObservable
    {
        private string name = String.Empty;

        private List<ErEntitySet> entitySets             = new(); // без всяких observablecollection - просто метод AddEntitySet привязываем куда это надо (к ноду, к графическому элементу и т.д.)
        private List<ErRelationshipSet> relationshipSets = new();
        private List<ErValueSet> valueSets               = new();
        private List<ErDiagram> diagrams                 = new();

        private readonly ObservableBase observableLogic = new();

        public readonly ErEntitySetWatcher EntitySetWatcher = new();
        public readonly ErRelationshipSetWatcher RelationshipSetWatcher = new();
        public readonly ErValueSetWatcher ValueSetWatcher = new();
        public readonly ErDiagramWatcher DiagramWatcher = new();

        public ErSchema(string name = "") 
        {
            this.name = name;

            this.Subscribe(EntitySetWatcher);
            this.Subscribe(RelationshipSetWatcher);
            this.Subscribe(ValueSetWatcher);
            this.Subscribe(DiagramWatcher);
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

            observableLogic.Notify(new ObjectCreatedNotification<ErEntitySet>(es));
            return es;
        }
        public ErEntitySet AddEntitySet(ErEntitySet entitySet)
        {
            ErEntitySet newEs = new(entitySet.Name);
            entitySets.Add(newEs);

            observableLogic.Notify(new ObjectCreatedNotification<ErEntitySet>(newEs));

            newEs.AddAttributeRange(entitySet.attributes);

            return newEs;
        }
        public void AddEntitySetRange(List<ErEntitySet> range)
        {
            foreach(var es in range)
            {
                this.AddEntitySet(es);
            }
        }
        public ErRelationshipSet AddRelationshipSet(string name = "")
        {
            ErRelationshipSet rs = new(name);
            relationshipSets.Add(rs);

            observableLogic.Notify(new ObjectCreatedNotification<ErRelationshipSet>(rs));
            return rs;
        }
        public void AddRelationshipSetRange(List<ErRelationshipSet> range)
        {
            relationshipSets.AddRange(range);

            foreach (var rs in range)
            {
                observableLogic.Notify(new ObjectCreatedNotification<ErRelationshipSet>(rs));
            }
        }
        public ErValueSet AddValueSet(string name = "")
        {
            ErValueSet vs = new(name);
            valueSets.Add(vs);

            observableLogic.Notify(new ObjectCreatedNotification<ErValueSet>(vs));
            return vs;
        }
        public void AddValueSetRange(List<ErValueSet> range)
        {
            valueSets.AddRange(range);

            foreach (var vs in range)
            {
                observableLogic.Notify(new ObjectCreatedNotification<ErValueSet>(vs));
            }
        }
        public ErDiagram AddDiagram(string name = "")
        {
            ErDiagram dgr = new(this, name);
            diagrams.Add(dgr);

            observableLogic.Notify(new ObjectCreatedNotification<ErDiagram>(dgr));
            return dgr;
        }
        public void AddDiagramRange(List<ErDiagram> range)
        {
            diagrams.AddRange(range);

            foreach (var dgr in range)
            {
                observableLogic.Notify(new ObjectCreatedNotification<ErDiagram>(dgr));
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
                foreach(var attr in el.attributes)
                {
                    output += $"\t ├ {attr.Name}\n";
                }

            }
            output += $"Множества связей (всего - {relationshipSets.Count}):\n";
            foreach (var el in relationshipSets)
            {
                output += $"\t{el.Name}\n";
                if(el.attributes.Count > 0) { output += $"\t\tAttributes:"; }
                foreach (var attr in el.attributes)
                {
                    output += $"\t ├ {attr.Name}\n";
                }

                if (el.roles.Count > 0) { output += $"\t\tRoles:"; }
                foreach (var role in el.roles)
                {
                    output += $"\t ├ {role.Name} (entity set {role.entitySet?.Name})\n";
                }
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
    }
}
