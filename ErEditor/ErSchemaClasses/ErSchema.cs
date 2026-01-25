using ErEditor.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public class ErSchema : IObservable, INamedObject
    {
        private string name = string.Empty;

        public readonly ErElementCollection<ErEntitySet> EntitySets;
        public readonly ErElementCollection<ErRelationshipSet> RelationshipSets;
        public readonly ErElementCollection<ErValueSet> ValueSets;
        public readonly ErElementCollection<ErDiagram> Diagrams;

        // Шлюзы для передачи уведомлений от схемы. Индивидуально для каждого типа элементов.
        // Изолируют передачу и обработку уведомлений от самого объекта. Являясь единой точкой, подписанной на все необходимые объекты.
        public readonly ErEntitySetWatcher EntitySetWatcher = new();
        public readonly ErRelationshipSetWatcher RelationshipSetWatcher = new();
        public readonly ErValueSetWatcher ValueSetWatcher = new();
        public readonly ErDiagramWatcher DiagramWatcher = new();

        public ErSchema(string name = "") 
        {
            this.name = name;

            EntitySets = new(EntitySetWatcher);
            RelationshipSets = new(RelationshipSetWatcher);
            ValueSets = new(ValueSetWatcher);
            Diagrams = new(DiagramWatcher);
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }
        public string PrintState()
        {
            string output = "";
            output += $"Множества сущностей (всего - {EntitySets.Count}):\n";
            foreach (var el in EntitySets)
            {
                output += $"\t{el.Name}\n";
                foreach(var attr in el.Attributes)
                {
                    output += $"\t ├ {attr.Name}\n";
                }

            }
            output += $"Множества связей (всего - {RelationshipSets.Count}):\n";
            foreach (var el in RelationshipSets)
            {
                output += $"\t{el.Name}\n";
                if(el.Attributes.Count > 0) { output += $"\t Attributes:\n"; }
                foreach (var attr in el.Attributes)
                {
                    output += $"\t ├ {attr.Name}\n";
                }

                if (el.Roles.Count > 0) { output += $"\t Roles:\n"; }
                foreach (var role in el.Roles)
                {
                    output += $"\t ├ {role.Name} -> entity set {role.EntitySet?.Name}\n";
                }

                if (el.Mappings.Count > 0) { output += $"\t Mappings:\n"; }
                foreach (var mapping in el.Mappings)
                {
                    output += $"\t ├ {mapping.Name} : ";
                    foreach(var role in mapping.PreImage)
                    {
                        output += $"{role.Name} x ";
                    }
                    output = output.Remove(output.Length - 3);
                    output += " -> ";
                    foreach (var role in mapping.Image)
                    {
                        output += $"{role.Name} x ";
                    }
                    output = output.Remove(output.Length - 3);
                    output += "\n";
                }
            }
            output += $"Множества значений (всего - {ValueSets.Count}):\n";
            foreach (var el in ValueSets)
            {
                output += $"\t{el.Name}\n";
            }
            output += $"Диаграммы (всего - {Diagrams.Count}):\n";
            foreach (var diagram in Diagrams)
            {
                output += $"\t{diagram.Name}\n";
                foreach (var primitive in diagram)
                {
                    output += $"\t ├ {ConsoleLog.GetShortTypeName(primitive)} of {ConsoleLog.GetShortTypeName(primitive.ErElement)} \"{primitive.ErElement.Name}\"\n";
                }
            }
            return output;
        }

        public bool Subscribe(IObserver observer)
        {
            return EntitySetWatcher.Subscribe(observer) 
                && RelationshipSetWatcher.Subscribe(observer)
                && ValueSetWatcher.Subscribe(observer)
                && DiagramWatcher.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return EntitySetWatcher.Unsubscribe(observer)
                && RelationshipSetWatcher.Unsubscribe(observer)
                && ValueSetWatcher.Unsubscribe(observer)
                && DiagramWatcher.Unsubscribe(observer);
        }
    }
}
