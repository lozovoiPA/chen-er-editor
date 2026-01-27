using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public abstract class ErElement : IObservable, INamedObject
    {
        protected string name = string.Empty;
        protected readonly ObservableBase observers = new();
        public bool BlockNotifying { get => observers.BlockNotifying; set => observers.BlockNotifying = value; }
        public virtual string Name
        {
            get { return name; }
            set { 
                name = value; 
                observers.Notify(new ObjectNameChangedNotification(this, name)); 
            }
        }

        public bool Subscribe(IObserver observer)
        {
            return observers.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observers.Unsubscribe(observer);
        }

        public override string ToString()
        {
            return name;
        }
    }

    public abstract class ErElementWithAttributes : ErElement
    {
        private List<ErAttribute> attributes = new();

        public ErAttribute AddAttribute(string name = "")
        {
            ErAttribute newAttribute = new(name);
            attributes.Add(newAttribute);

            observers.Notify(new ObjectAddedNotification<ErElementWithAttributes, ErAttribute>(this, newAttribute));
            return newAttribute;
        }
        public void AddAttributeRange(IEnumerable<ErAttribute> range)
        {
            foreach(var attr in range)
            {
                this.AddAttribute(attr.Name);
            }
        }
        public bool RemoveAttribute(ErAttribute attribute)
        {
            bool result = attributes.Remove(attribute);
            if (result)
            {
                observers.Notify(new ObjectDeletedNotification<ErAttribute>(attribute));
            }
            return result;
        }
        public ReadOnlyCollection<ErAttribute> Attributes
        {
            get { return attributes.AsReadOnly(); }
        }
    }

    public class ErEntitySet : ErElementWithAttributes
    {
        public static readonly ErEntitySet Empty = new ErEntitySet();

        public ErEntitySet() { }
        public ErEntitySet(string name)
        {
            this.name = name;
        }
    }

    public class ErRelationshipSet : ErElementWithAttributes
    {
        private List<ErRole> roles = new();
        private List<ErMapping> mappings = new();

        public ErRelationshipSet() { }
        public ErRelationshipSet(string name)
        {
            this.name = name;
        }

        public ReadOnlyCollection<ErRole> Roles
        {
            get { return roles.AsReadOnly(); }
        }
        public ReadOnlyCollection<ErMapping> Mappings
        {
            get { return mappings.AsReadOnly(); }
        }

        public ErRole AddRole(string name = "", bool addMapping = false)
        {
            var newRole = this.AddRole(ErEntitySet.Empty, name, addMapping);
            return newRole;
        }
        public ErRole AddRole(ErEntitySet entitySet, string name = "", bool addMapping = false)
        {
            ErRole newRole = new(entitySet, name);

            // updating existing mappings
            foreach (var mapping in mappings)
            {
                mapping.AddToImage(newRole);
            }

            // add new map
            if (addMapping && this.roles.Count >= 1)
            {
                // В случае с числом ролей 2 у нас обратное и прямое в одном объекте, но при трех и более уже нет
                if (this.roles.Count == 2)
                {
                    ErMapping newMapping2 = new();
                    ErRole role2 = mappings[0].Image[0];
                    newMapping2.AddToPreImage(role2);
                    foreach (var role in roles)
                    {
                        if(role != role2)
                        {
                            newMapping2.AddToImage(role);
                        }
                    }
                    newMapping2.AddToImage(newRole);

                    mappings.Add(newMapping2);
                    observers.Notify(new ObjectAddedNotification<ErRelationshipSet, ErMapping>(this, newMapping2));
                }

                ErMapping newMapping = new();

                newMapping.AddToPreImage(newRole);
                foreach (var role in roles)
                {
                    newMapping.AddToImage(role);
                }

                mappings.Add(newMapping);
                observers.Notify(new ObjectAddedNotification<ErRelationshipSet, ErMapping>(this, newMapping));
            }

            roles.Add(newRole);
            observers.Notify(new ObjectAddedNotification<ErRelationshipSet, ErRole>(this, newRole));

            return newRole;
        }
        public bool RemoveRole(ErRole role)
        {
            bool result = roles.Remove(role);

            if (result)
            {
                observers.Notify(new ObjectDeletedNotification<ErRole>(role));

                List<int> mappingIndicesForDeletion = new();
                mappings.ForEach(mapping =>
                {
                    if (mapping.Remove(role) == null)
                    {
                        mappingIndicesForDeletion.Add(mappings.IndexOf(mapping));
                        observers.Notify(new ObjectDeletedNotification<ErMapping>(mapping));
                    }
                });
                mappingIndicesForDeletion.ForEach(indice => mappings.RemoveAt(indice));
                if (mappings.Count == 2)
                {
                    var mapping = mappings[1];
                    mappings.RemoveAt(1);
                    observers.Notify(new ObjectDeletedNotification<ErMapping>(mapping));
                }
            }

            return result;
        }
        public ErMapping AddMapping(string name = "")
        {
            ErMapping newMapping = new(name);
            mappings.Add(newMapping);
            observers.Notify(new ObjectAddedNotification<ErRelationshipSet, ErMapping>(this, newMapping));
            return newMapping;
        }
    }

    public class ErValueSet : ErElement
    {
        public ErValueSet() { }
        public ErValueSet(string name)
        {
            this.name = name;
        }
    }
}
