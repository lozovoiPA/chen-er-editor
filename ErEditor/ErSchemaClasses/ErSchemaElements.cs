using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public interface IErElement : IObservable { }
    public abstract class ErElement : IErElement
    {
        protected string name = String.Empty;
        protected readonly ObservableBase observableLogic = new();

        public virtual string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification(this, name)); }
        }

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

            observableLogic.Notify(new ObjectAddedNotification<ErElementWithAttributes, ErAttribute>(this, newAttribute));
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

        public ErEntitySet(string name = "")
        {
            this.name = name;
        }
    }

    public class ErRelationshipSet : ErElementWithAttributes
    {
        private List<ErRole> roles = new();
        private List<ErMapping> mappings = new();

        public ErRelationshipSet(string name = "")
        {
            this.name = name;
        }

        public ErRole AddRole(string name = "")
        {
            var newRole = this.AddRole(ErEntitySet.Empty, name);

            return newRole;
        }
        public ErRole AddRole(ErEntitySet entitySet, string name = "")
        {
            ErRole newRole = new(entitySet, name);
            roles.Add(newRole);

            observableLogic.Notify(new ObjectAddedNotification<ErRelationshipSet, ErRole>(this, newRole));

            return newRole;
        }
        public bool RemoveRole(ErRole role)
        {
            if (!roles.Contains(role))
            {
                return false;
            }
            roles.Remove(role);
            observableLogic.Notify(new ObjectDeletedNotification<ErRole>(role));
            return true;
        }

        public ErMapping AddMapping(string name = "")
        {
            ErMapping newMapping = new(name);
            mappings.Add(newMapping);
            return newMapping;
        }


        public ReadOnlyCollection<ErRole> Roles
        {
            get { return roles.AsReadOnly(); }
        }
        public ReadOnlyCollection<ErMapping> Mappings
        {
            get { return mappings.AsReadOnly(); }
        }
    }

    public class ErValueSet : ErElement
    {
        public ErValueSet(string name = "")
        {
            this.name = name;
        }
    }
}
