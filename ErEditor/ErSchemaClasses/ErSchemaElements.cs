using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            // пока костыльно пипеццццццц...........................ну и ладно.
            // add new map
            if (addMapping && this.roles.Count >= 1)
            {
                // В случае с числом ролей 2 у нас обратное и прямое в одном объекте, но при трех и более уже нет
                if (this.roles.Count == 2)
                {
                    ErMapping newMapping2 = new();

                    newMapping2.AddToPreImage(roles[0]);
                    foreach (var role in roles)
                    {
                        if(role != roles[0])
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
            if (!roles.Contains(role))
            {
                return false;
            }
            roles.Remove(role);
            observers.Notify(new ObjectDeletedNotification<ErRole>(role));
            return true;
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
